
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
        private readonly RedSocialContext _context;

        public CommentsController(RedSocialContext context)
        {
            _context = context;
        }

        // GET: api/Comments/post/5
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            return await _context.Comments.Where(c => c.PostID == postId).ToListAsync();
        }

        // POST: api/Comments
        [Authorize] // Requiere autenticación
        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment(Comment comment)
        {
            var post = await _context.Posts.FindAsync(comment.PostID);
            if (post == null)
            {
                return NotFound("El post asociado no existe");
            }

            comment.FechaCreacion = DateTime.Now;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCommentsByPost", new { postId = comment.PostID }, comment);
        }

        // DELETE: api/Comments/5
        [Authorize] // Requiere autenticación
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
