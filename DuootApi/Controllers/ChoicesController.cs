
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChoicesController : ControllerBase
    {
        private readonly RedSocialContext _context;

        public ChoicesController(RedSocialContext context)
        {
            _context = context;
        }

        // GET: api/Choices/5
        [HttpGet("{postId}")]
        public async Task<ActionResult<IEnumerable<Choice>>> GetChoices(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            return await _context.Choices.Where(c => c.PostID == postId).ToListAsync();
        }

        // POST: api/Choices
        [Authorize] // Requiere autenticación
        [HttpPost]
        public async Task<ActionResult<Choice>> CreateChoice(Choice choice)
        {
            var post = await _context.Posts.FindAsync(choice.PostID);
            if (post == null)
            {
                return NotFound("El post asociado no existe");
            }

            if (choice.NumeroOpcion != 1 && choice.NumeroOpcion != 2)
            {
                return BadRequest("El número de opción debe ser 1 o 2");
            }

            // Verificar si ya existen opciones 1 y 2 para el post
            if (await _context.Choices.AnyAsync(c => c.PostID == choice.PostID && c.NumeroOpcion == choice.NumeroOpcion))
            {
                return Conflict("La opción ya existe para este post");
            }

            _context.Choices.Add(choice);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetChoices", new { postId = choice.PostID }, choice);
        }

        // DELETE: api/Choices/5
        [Authorize] // Requiere autenticación
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChoice(int id)
        {
            var choice = await _context.Choices.FindAsync(id);
            if (choice == null)
            {
                return NotFound();
            }

            _context.Choices.Remove(choice);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
