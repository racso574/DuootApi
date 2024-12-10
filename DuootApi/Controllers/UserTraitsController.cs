using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protege todos los endpoints del controlador
    public class UserTraitsController : ControllerBase
    {
        private readonly DuootDbContext _context;

        public UserTraitsController(DuootDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene las personalidades de un usuario específico.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de personalidades del usuario</returns>
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<PersonalityTrait>>> GetUserTraits(int userId)
        {
            // Verificar si el usuario existe
            var userExists = await _context.Users.AnyAsync(u => u.UserID == userId);
            if (!userExists)
            {
                return NotFound($"Usuario con ID {userId} no encontrado.");
            }

            // Obtener las personalidades del usuario
            var userTraits = await _context.UserTraits
                .Where(ut => ut.UserID == userId)
                .Include(ut => ut.PersonalityTrait)
                .Select(ut => ut.PersonalityTrait)
                .ToListAsync();

            return Ok(userTraits);
        }

        /// <summary>
        /// Obtiene la lista de personalidades posibles.
        /// </summary>
        /// <returns>Lista completa de personalidades disponibles</returns>
        [HttpGet("Available")]
        public async Task<ActionResult<IEnumerable<PersonalityTrait>>> GetAvailableTraits()
        {
            var availableTraits = await _context.PersonalityTraits.ToListAsync();
            return Ok(availableTraits);
        }

        /// <summary>
        /// Añade una o más personalidades al usuario autenticado.
        /// </summary>
        /// <param name="traitIds">Lista de IDs de personalidades a añadir</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut]
        public async Task<IActionResult> AddUserTraits([FromBody] List<int> traitIds)
        {
            if (traitIds == null || !traitIds.Any())
            {
                return BadRequest("La lista de IDs de personalidades no puede estar vacía.");
            }

            // Obtener el userId del token JWT
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
            {
                return Unauthorized("No se pudo determinar el usuario autenticado.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("ID de usuario inválido en el token.");
            }

            // Verificar si el usuario existe
            var user = await _context.Users
                .Include(u => u.UserTraits)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound($"Usuario con ID {userId} no encontrado.");
            }

            // Verificar si las personalidades existen
            var existingTraits = await _context.PersonalityTraits
                .Where(pt => traitIds.Contains(pt.TraitID))
                .ToListAsync();

            var invalidTraitIds = traitIds.Except(existingTraits.Select(pt => pt.TraitID)).ToList();
            if (invalidTraitIds.Any())
            {
                return BadRequest($"Las personalidades con los IDs {string.Join(", ", invalidTraitIds)} no existen.");
            }

            // Filtrar las personalidades que ya están asociadas al usuario
            var existingUserTraitIds = user.UserTraits.Select(ut => ut.TraitID).ToHashSet();
            var newTraitIds = traitIds.Where(id => !existingUserTraitIds.Contains(id)).ToList();

            if (!newTraitIds.Any())
            {
                return BadRequest("Todas las personalidades proporcionadas ya están asociadas al usuario.");
            }

            // Crear nuevas asociaciones UserTrait
            var newUserTraits = newTraitIds.Select(id => new UserTrait
            {
                UserID = userId,
                TraitID = id
            });

            _context.UserTraits.AddRange(newUserTraits);
            await _context.SaveChangesAsync();

            return Ok("Personalidades añadidas exitosamente.");
        }
    }
}
