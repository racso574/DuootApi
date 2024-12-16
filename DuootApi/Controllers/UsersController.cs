using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;    // Para opciones de validación de tokens JWT
using System.IdentityModel.Tokens.Jwt;   // Para generar tokens JWT
using System.Security.Claims;            // Para trabajar con claims JWT
using Microsoft.AspNetCore.Authorization; // Para usar [Authorize] y proteger endpoints
using Microsoft.AspNetCore.Http;
using System.IO;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DuootDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _imagesDirectoryPath;

        public UsersController(DuootDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            // Definir el directorio donde se guardarán las imágenes
            _imagesDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            Directory.CreateDirectory(_imagesDirectoryPath); // Crea la carpeta si no existe
            Console.WriteLine($"Images directory path: {_imagesDirectoryPath}");
        }

        // POST: api/Users/register - Registrar un nuevo usuario con una imagen de perfil opcional
        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<User>> RegisterUser(
            [FromForm] string Email,
            [FromForm] string Username,
            [FromForm] string Password,
            [FromForm] IFormFile ProfileImage
        )
        {
            // Validar campos requeridos
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                return BadRequest("Email, Username y Password son obligatorios.");
            }

            // Verificar si el email o username ya existen
            if (await _context.Users.AnyAsync(u => u.Email == Email || u.Username == Username))
            {
                return Conflict("El email o el username ya existen.");
            }

            // Inicializar la entidad de usuario
            var user = new User
            {
                Email = Email,
                Username = Username,
                PasswordHash = HashPassword(Password),
     
            };

            // Manejar la carga de la imagen de perfil si se proporciona
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Validar el tipo de archivo y el tamaño
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(ProfileImage.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Formato de imagen inválido. Los formatos permitidos son .jpg, .jpeg, .png, .gif.");
                }

                if (ProfileImage.Length > 5 * 1024 * 1024) // Límite de 5 MB
                {
                    return BadRequest("El tamaño de la imagen excede el límite de 5MB.");
                }

                // Generar un nombre de archivo único
                string imageFileName = Guid.NewGuid().ToString() + extension;
                string imagePath = Path.Combine(_imagesDirectoryPath, imageFileName);

                try
                {
                    // Guardar la imagen en el servidor
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    // Asignar la URL relativa a la propiedad ProfileImage del usuario
                    user.ProfileImage = "/Images/" + imageFileName;
                    Console.WriteLine($"Imagen de perfil guardada: {user.ProfileImage}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar la imagen de perfil: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error al guardar la imagen de perfil.");
                }
            }

            // Agregar el usuario a la base de datos
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserID }, user);
        }

        // POST: api/Users/login - Inicio de sesión de usuario
        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginUser([FromBody] LoginRequest loginRequest)
        {
            // Buscar al usuario por email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized("Email o contraseña incorrectos.");
            }

            // Generar el token JWT
            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        // Método para generar un token JWT
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userID", user.UserID.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // GET: api/Users/5 - Obtener usuario por ID
        [Authorize] // Requiere autenticación
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // DELETE: api/Users/5 - Eliminar un usuario
        [Authorize] // Requiere autenticación
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Opcional: Eliminar la imagen de perfil si existe
            if (!string.IsNullOrWhiteSpace(user.ProfileImage))
            {
                var imageName = Path.GetFileName(user.ProfileImage);
                var imagePath = Path.Combine(_imagesDirectoryPath, imageName);
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                        Console.WriteLine($"Imagen de perfil eliminada: {imagePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al eliminar la imagen de perfil: {ex.Message}");
                        // No retornamos un error aquí para no interrumpir el flujo principal
                    }
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Nuevo Endpoint: GET: api/Users/profileImage - Obtener la URL de la imagen de perfil del usuario autenticado
        [Authorize] // Requiere autenticación
        [HttpGet("profileImage")]
        public async Task<ActionResult<string>> GetProfileImage()
        {
            // Obtener el UserID del token JWT
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized("No se encontró el UserID en el token.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("UserID inválido en el token.");
            }

            // Buscar al usuario en la base de datos
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Devolver la URL de la imagen de perfil
            if (string.IsNullOrWhiteSpace(user.ProfileImage))
            {
                return NotFound("El usuario no tiene una imagen de perfil.");
            }

            return Ok(new { ProfileImageUrl = user.ProfileImage });
        }

        // Método para verificar si un usuario existe
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        // Método para hashear una contraseña
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Método para verificar una contraseña
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInputPassword = HashPassword(password);
            return hashedInputPassword == hashedPassword;
        }

        // GET: api/Users/5/Username - Obtener el username por ID sin autenticación
        [HttpGet("{id}/Username")]
        public async Task<ActionResult<string>> GetUsernameById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return Ok(user.Username);
        }

        // GET: api/Users/me - Obtener datos del usuario autenticado a partir del token
// GET: api/Users/me - Obtener datos del usuario autenticado a partir del token
[Authorize] // Requiere autenticación
[HttpGet("me")]
public async Task<ActionResult<User>> GetUserFromToken()
{
    // Obtener el UserID del token JWT
    var userIdClaim = User.FindFirst("userID");
    if (userIdClaim == null)
    {
        return Unauthorized("No se encontró el UserID en el token.");
    }

    if (!int.TryParse(userIdClaim.Value, out int userId))
    {
        return Unauthorized("UserID inválido en el token.");
    }

    // Aquí realizamos el eager loading de UserTraits y PersonalityTrait
    var user = await _context.Users
        .Include(u => u.UserTraits)
        .ThenInclude(ut => ut.PersonalityTrait) // Carga la descripción del trait
        .FirstOrDefaultAsync(u => u.UserID == userId);

    if (user == null)
    {
        return NotFound("Usuario no encontrado.");
    }

    return Ok(user);
}

    }

    // Clases auxiliares para solicitudes
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Nota: La clase UpdatePasswordRequest ya no es necesaria y puede ser eliminada si no se usa en otro lugar.
}
