using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly DuootDbContext _context;

        public CommentsController(DuootDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAllComments()
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreationDate)
                .ToListAsync();

            return Ok(comments);
        }


        // GET: api/Comments/Post/5 - Obtener comentarios de un post específico
        [HttpGet("Post/{postId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound("El post no existe.");
            }

            var comments = await _context.Comments
                .Where(c => c.PostID == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreationDate)
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/Comments/Post/5 - Agregar un comentario a un post
        [Authorize]
        [HttpPost("Post/{postId}")]
        public async Task<ActionResult<Comment>> AddCommentToPost(int postId, [FromBody] Comment comment)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound("El post no existe.");
            }

            // Obtener el UserID del token JWT
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            // Asignar el UserID, PostID y la fecha de creación al comentario
            comment.UserID = userId;
            comment.PostID = postId;
            comment.CreationDate = DateTime.UtcNow;

            // Asegurarse de que CommentID esté en 0 para que EF lo considere como nuevo
            comment.CommentID = 0;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Cargar la información del usuario
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();

            return CreatedAtAction(nameof(GetCommentsByPost), new { postId = postId }, comment);
        }
        
        [HttpDelete]    
        public async Task<IActionResult> DeleteAllComments()
        {
            var comments = await _context.Comments.ToListAsync();

            if (!comments.Any())
            {
                return NotFound("No hay comentarios para eliminar.");
            }

            _context.Comments.RemoveRange(comments);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    
    
    }
}

