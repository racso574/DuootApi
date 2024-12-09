using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly DuootDbContext _context;

        public PostsController(DuootDbContext context)
        {
            _context = context;
        }

        // GET: api/Posts - Obtener todos los posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)         // Incluye información del usuario que creó el post
                .Include(p => p.Choices)      // Incluye las opciones del post
                .Include(p => p.Comments)     // Incluye los comentarios del post
                .ToListAsync();

            return Ok(posts);
        }

        // GET: api/Posts/5 - Obtener un post específico por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Choices)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.PostID == id);

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        // POST: api/Posts - Crear un nuevo post
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost([FromBody] Post post)
        {
            // Obtener el UserID del token JWT
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            // Asignar el UserID y la fecha de creación
            post.UserID = userId;
            post.CreationDate = DateTime.UtcNow;

            // Validar y asignar los choices si existen
            if (post.Choices != null)
            {
                foreach (var choice in post.Choices)
                {
                    // Asegurarse de que los ChoiceID no estén establecidos
                    choice.ChoiceID = 0;
                }
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPost), new { id = post.PostID }, post);
        }

        // GET: api/Posts/5/Choices - Obtener todos los choices de un post específico
        [HttpGet("{postId}/Choices")]
        public async Task<ActionResult<IEnumerable<Choice>>> GetChoicesForPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound("El post no existe.");
            }

            var choices = await _context.Choices
                .Where(c => c.PostID == postId)
                .ToListAsync();

            return Ok(choices);
        }

        // GET: api/Posts/5/Choices/3 - Obtener un choice específico de un post
        [HttpGet("{postId}/Choices/{choiceId}")]
        public async Task<ActionResult<Choice>> GetChoiceForPost(int postId, int choiceId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound("El post no existe.");
            }

            var choice = await _context.Choices
                .FirstOrDefaultAsync(c => c.PostID == postId && c.ChoiceID == choiceId);

            if (choice == null)
            {
                return NotFound("El choice no existe en este post.");
            }

            return Ok(choice);
        }

        // POST: api/Posts/5/Choices - Crear un choice para un post específico
        [Authorize]
        [HttpPost("{postId}/Choices")]
        public async Task<ActionResult<Choice>> CreateChoiceForPost(int postId, [FromBody] Choice choice)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound("El post no existe.");
            }

            // Verificar si el usuario autenticado es el propietario del post
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            if (post.UserID != userId)
            {
                return Forbid("No tienes permiso para agregar choices a este post.");
            }

            // Asignar el PostID al choice
            choice.PostID = postId;

            _context.Choices.Add(choice);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChoiceForPost), new { postId = postId, choiceId = choice.ChoiceID }, choice);
        }

        // PUT: api/Posts/5/Choices/3 - Actualizar un choice de un post específico
        [Authorize]
        [HttpPut("{postId}/Choices/{choiceId}")]
        public async Task<IActionResult> UpdateChoiceForPost(int postId, int choiceId, [FromBody] Choice updatedChoice)
        {
            if (choiceId != updatedChoice.ChoiceID)
            {
                return BadRequest("El ID del choice no coincide.");
            }

            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound("El post no existe.");
            }

            var existingChoice = await _context.Choices
                .FirstOrDefaultAsync(c => c.PostID == postId && c.ChoiceID == choiceId);

            if (existingChoice == null)
            {
                return NotFound("El choice no existe en este post.");
            }

            // Verificar si el usuario autenticado es el propietario del post
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            if (post.UserID != userId)
            {
                return Forbid("No tienes permiso para actualizar choices de este post.");
            }

            // Actualizar los campos necesarios
            existingChoice.TextContent = updatedChoice.TextContent;
            existingChoice.ImageURL = updatedChoice.ImageURL;
            existingChoice.ChoiceNumber = updatedChoice.ChoiceNumber;

            _context.Entry(existingChoice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChoiceExists(choiceId))
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

        // DELETE: api/Posts/5/Choices/3 - Eliminar un choice de un post específico
        [Authorize]
        [HttpDelete("{postId}/Choices/{choiceId}")]
        public async Task<IActionResult> DeleteChoiceForPost(int postId, int choiceId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound("El post no existe.");
            }

            var choice = await _context.Choices
                .FirstOrDefaultAsync(c => c.PostID == postId && c.ChoiceID == choiceId);

            if (choice == null)
            {
                return NotFound("El choice no existe en este post.");
            }

            // Verificar si el usuario autenticado es el propietario del post
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            if (post.UserID != userId)
            {
                return Forbid("No tienes permiso para eliminar choices de este post.");
            }

            _context.Choices.Remove(choice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChoiceExists(int id)
        {
            return _context.Choices.Any(e => e.ChoiceID == id);
        }
    }
}
