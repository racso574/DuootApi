
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotesController : ControllerBase
    {
        private readonly RedSocialContext _context;

        public VotesController(RedSocialContext context)
        {
            _context = context;
        }

        // GET: api/Votes/post/5
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<Vote>>> GetVotesByPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            return await _context.Votes.Where(v => v.PostID == postId).ToListAsync();
        }

        // POST: api/Votes
        [Authorize] // Requiere autenticación
        [HttpPost]
        public async Task<ActionResult<Vote>> CreateVote(Vote vote)
        {
            var post = await _context.Posts.FindAsync(vote.PostID);
            if (post == null)
            {
                return NotFound("El post asociado no existe");
            }

            var choice = await _context.Choices.FindAsync(vote.ChoiceID);
            if (choice == null || choice.PostID != vote.PostID)
            {
                return BadRequest("La opción no es válida para este post");
            }

            if (await _context.Votes.AnyAsync(v => v.UserID == vote.UserID && v.PostID == vote.PostID))
            {
                return Conflict("El usuario ya ha votado en este post");
            }

            vote.FechaVoto = DateTime.Now;
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVotesByPost", new { postId = vote.PostID }, vote);
        }

        // DELETE: api/Votes/5
        [Authorize] // Requiere autenticación
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVote(int id)
        {
            var vote = await _context.Votes.FindAsync(id);
            if (vote == null)
            {
                return NotFound();
            }

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
