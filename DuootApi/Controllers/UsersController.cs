using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;    // To work with JWT token validation options
using System.IdentityModel.Tokens.Jwt;   // To generate JWT token
using System.Security.Claims;            // To work with JWT claims
using Microsoft.AspNetCore.Authorization; // To use [Authorize] and protect endpoints

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DuootDbContext _context;
        private readonly IConfiguration _configuration;  // Add IConfiguration to get JWT settings

        public UsersController(DuootDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;  // Inject configuration
        }

        // POST: api/Users - Register a new user
        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(User user)
        {
            // Check if the email or username already exists
            if (await _context.Users.AnyAsync(u => u.Email == user.Email || u.Username == user.Username))
            {
                return Conflict("The email or username already exists");
            }

            // Hash the password
            user.PasswordHash = HashPassword(user.PasswordHash);

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserID }, user);
        }

        // POST: api/Users/login - User login
        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginUser([FromBody] LoginRequest loginRequest)
        {
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized("Incorrect email or password");
            }

            // Generate the JWT token
            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        // Method to generate a JWT token
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

        // GET: api/Users/5
        [Authorize] // Requires authentication
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

        // PUT: api/Users/5/username - Edit user username
        [Authorize] // Requires authentication
        [HttpPut("{id}/username")]
        public async Task<IActionResult> UpdateUsername(int id, [FromBody] string newUsername)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.Username = newUsername;
            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Users/5/password - Edit user password
        [Authorize] // Requires authentication
        [HttpPut("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordRequest updatePasswordRequest)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Verify the current password is correct
            if (!VerifyPassword(updatePasswordRequest.CurrentPassword, existingUser.PasswordHash))
            {
                return Unauthorized("Current password is incorrect");
            }

            // Update to the new password
            existingUser.PasswordHash = HashPassword(updatePasswordRequest.NewPassword);
            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Users/5/profileImage - Edit user profile image
        [Authorize] // Requires authentication
        [HttpPut("{id}/profileImage")]
        public async Task<IActionResult> UpdateProfileImage(int id, [FromBody] string newProfileImage)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.ProfileImage = newProfileImage;
            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5 - Delete user
        [Authorize] // Requires authentication
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Method to verify if a user exists
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        // Method to hash a password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Method to verify a password
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInputPassword = HashPassword(password);
            return hashedInputPassword == hashedPassword;
        }

        // GET: api/Users/5/username - Get username by ID without authentication
[HttpGet("{id}/Username")]
public async Task<ActionResult<string>> GetUsernameById(int id)
{
    var user = await _context.Users.FindAsync(id);

    if (user == null)
    {
        return NotFound("User not found");
    }

    return Ok(user.Username);
}

    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    
}
