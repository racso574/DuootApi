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
    public class VotesController : ControllerBase
    {
        private readonly DuootDbContext _context;

        public VotesController(DuootDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el conteo de votos de cada opción para un post específico.
        /// </summary>
        [HttpGet("Post/{postId}")]
        public async Task<ActionResult> GetVotesForPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = "El post no existe." });
            }

            var votesGrouped = await _context.Votes
                .Where(v => v.PostID == postId)
                .GroupBy(v => v.ChoiceID)
                .Select(g => new 
                {
                    ChoiceID = g.Key,
                    VoteCount = g.Count()
                })
                .ToListAsync();

            return Ok(votesGrouped);
        }

        /// <summary>
        /// Registra un voto de un usuario autenticado a una opción específica de un post.
        /// </summary>
        [Authorize]
        [HttpPost("Post/{postId}/Choice/{choiceId}")]
        public async Task<ActionResult> Vote(int postId, int choiceId)
        {
            var post = await _context.Posts.Include(p => p.Choices).FirstOrDefaultAsync(p => p.PostID == postId);
            if (post == null)
            {
                return NotFound(new { message = "El post no existe." });
            }

            var choice = post.Choices.FirstOrDefault(c => c.ChoiceID == choiceId);
            if (choice == null)
            {
                return NotFound(new { message = "La opción no existe en este post." });
            }

            // Obtener el userID del token
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "No autorizado." });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "UserID inválido en el token." });
            }

            // (Opcional) Verificar si el usuario ya votó en este post
            var existingVote = await _context.Votes.FirstOrDefaultAsync(v => v.PostID == postId && v.UserID == userId);
            if (existingVote != null)
            {
                // Si no permites cambiar el voto:
                // return Conflict(new { message = "Ya has votado en este post." });

                // Si permites cambiar el voto:
                existingVote.ChoiceID = choiceId;
                existingVote.VoteDate = DateTime.UtcNow;
                _context.Entry(existingVote).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Voto actualizado correctamente." });
            }

            // Si es la primera vez que vota
            var vote = new Vote
            {
                UserID = userId,
                PostID = postId,
                ChoiceID = choiceId,
                VoteDate = DateTime.UtcNow
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Voto registrado correctamente." });
        }

        /// <summary>
        /// Obtiene los posts en los que el usuario autenticado ha votado.
        /// </summary>
        [Authorize]
        [HttpGet("UserVotes")]
        public async Task<ActionResult> GetUserVotes()
        {
            // Obtener el userID del token
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "No autorizado." });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "UserID inválido en el token." });
            }

            // Consultar los votos del usuario, incluyendo detalles de Post y Choice
            var userVotes = await _context.Votes
                .Where(v => v.UserID == userId)
                .Include(v => v.Post)
                .Include(v => v.Choice)
                .Select(v => new
                {
                    v.PostID,
                    PostTitle = v.Post.Title, // Asegúrate de que la entidad Post tiene una propiedad Title
                    v.ChoiceID,
                    v.VoteDate
                })
                .ToListAsync();

            return Ok(userVotes);
        }
    }
}
