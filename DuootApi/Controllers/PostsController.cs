using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly DuootDbContext _context;
        private readonly string _imagesDirectoryPath;

        public PostsController(DuootDbContext context)
        {
            _context = context;
            // Definir el directorio donde se guardarán las imágenes
            _imagesDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            Directory.CreateDirectory(_imagesDirectoryPath); // Crea la carpeta si no existe
            Console.WriteLine($"Images directory path: {_imagesDirectoryPath}");
        }

        // GET: api/Posts - Obtener todos los posts con sus choices y categorías
       // GET: api/Posts - Obtener todos los posts con sus choices, categorías y username del creador
[HttpGet]
public async Task<ActionResult<IEnumerable<object>>> GetPosts()
{
    Console.WriteLine("Fetching posts with choices, categories, and user...");

    var posts = await _context.Posts
        .Include(p => p.Choices)
        .Include(p => p.PostCategories)
            .ThenInclude(pc => pc.Category)
        .Include(p => p.User) // Incluir la entidad User
        .ToListAsync();

    var result = posts.Select(p => new
    {
        p.PostID,
        p.UserID,
        Username = p.User.Username, // Incluir solo el Username
        p.Title,
        p.CreationDate,
        p.Description,
        p.Choices,
        p.PostCategories
    });

    return Ok(result);
}


        [HttpGet("bycategory")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByCategory([FromQuery] int categoryId)
        {
            var posts = await _context.Posts
                .Include(p => p.Choices)
                .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category)
                .Where(p => p.PostCategories.Any(pc => pc.CategoryID == categoryId))
                .ToListAsync();

            return Ok(posts);
        }

        // POST: api/Posts - Crear un nuevo post con sus choices, sus imágenes y categorías
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Post>> CreatePost(
            [FromForm] string Title,
            [FromForm] string Description,
            [FromForm] List<string> ChoicesTextContent,
            [FromForm] List<IFormFile> ChoicesImages,
            [FromForm] List<int> CategoryIds
        )
        {
            // Obtener el UserID del token JWT
            var userIdClaim = User.FindFirst("userID");
            if (userIdClaim == null)
            {
                return Unauthorized("No se pudo determinar el usuario.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("UserID inválido en el token.");
            }

            Console.WriteLine("Starting CreatePost...");
            Console.WriteLine($"Title: {Title}, Description: {Description}");
            Console.WriteLine($"ChoicesTextContent count: {ChoicesTextContent?.Count}, ChoicesImages count: {ChoicesImages?.Count}");
            Console.WriteLine($"CategoryIds count: {CategoryIds?.Count}");

            var post = new Post
            {
                UserID = userId, // Asignar el UserID obtenido del token
                Title = Title,
                Description = Description,
                CreationDate = DateTime.UtcNow,
                Choices = new List<Choice>(),
                PostCategories = new List<PostCategory>()
            };

            // Agregar choices
            for (int i = 0; i < ChoicesTextContent.Count; i++)
            {
                var choiceText = ChoicesTextContent[i];
                var imageFile = (ChoicesImages != null && ChoicesImages.Count > i) ? ChoicesImages[i] : null;

                Console.WriteLine($"Processing choice {i + 1}: Text: {choiceText}, Image: {(imageFile != null ? imageFile.FileName : "No image")}");

                var choice = new Choice
                {
                    TextContent = choiceText,
                    ChoiceNumber = i + 1
                };

                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var imagePath = Path.Combine(_imagesDirectoryPath, imageFileName);

                        Console.WriteLine($"Saving image to: {imagePath}");
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        choice.ImageURL = "/Images/" + imageFileName;
                        Console.WriteLine($"Image saved. Relative path: {choice.ImageURL}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving image: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"No image provided for choice {i + 1}");
                }

                post.Choices.Add(choice);
            }

            // Asignar categorías
            foreach (var catId in CategoryIds)
            {
                post.PostCategories.Add(new PostCategory
                {
                    CategoryID = catId
                });
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Post created with ID: {post.PostID}");
            return CreatedAtAction(nameof(GetPosts), new { id = post.PostID }, post);
        }

        // DELETE: api/Posts - Eliminar todos los posts y sus imágenes
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllPosts()
        {
            Console.WriteLine("Deleting all posts...");

            // Obtener todos los posts con sus Choices para eliminar las imágenes
            var allPosts = await _context.Posts
                .Include(p => p.Choices)
                .ToListAsync();

            // Eliminar las imágenes asociadas a los Choices
            foreach (var post in allPosts)
            {
                foreach (var choice in post.Choices)
                {
                    if (!string.IsNullOrWhiteSpace(choice.ImageURL))
                    {
                        // choice.ImageURL es algo como "/Images/<nombrearchivo>"
                        // Necesitamos el nombre del archivo
                        var imageName = Path.GetFileName(choice.ImageURL);
                        var imagePath = Path.Combine(_imagesDirectoryPath, imageName);

                        if (System.IO.File.Exists(imagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(imagePath);
                                Console.WriteLine($"Deleted image: {imagePath}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error deleting image: {ex.Message}");
                            }
                        }
                    }
                }
            }

            // Eliminar todos los posts
            // Esto debería eliminar en cascada Choices, Comments, Votes, PostCategories
            // siempre y cuando la relación esté configurada con cascada en el modelo
            _context.Posts.RemoveRange(allPosts);
            await _context.SaveChangesAsync();

            Console.WriteLine("All posts and related data have been deleted.");

            return NoContent();
        }
    }
}
