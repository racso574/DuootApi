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

        // GET: api/Posts - Obtener todos los posts con sus choices (incluyendo rutas de imagen)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            Console.WriteLine("Fetching posts with choices...");
            var posts = await _context.Posts
                .Include(p => p.Choices)
                .ToListAsync();

            foreach (var post in posts)
            {
                Console.WriteLine($"Post ID: {post.PostID}, Title: {post.Title}");
                foreach (var choice in post.Choices)
                {
                    Console.WriteLine($"Choice ID: {choice.ChoiceID}, Text: {choice.TextContent}, Image URL: {choice.ImageURL}");
                }
            }

            return Ok(posts);
        }

        // POST: api/Posts - Crear un nuevo post con sus choices y sus imágenes
        // Se fuerza temporalmente el UserID a 1
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Post>> CreatePost(
            [FromForm] string Title,
            [FromForm] string Description,
            [FromForm] List<string> ChoicesTextContent,
            [FromForm] List<IFormFile> ChoicesImages
        )
        {
            Console.WriteLine("Starting CreatePost...");
            Console.WriteLine($"Title: {Title}, Description: {Description}");
            Console.WriteLine($"ChoicesTextContent count: {ChoicesTextContent?.Count}, ChoicesImages count: {ChoicesImages?.Count}");

            // Crear un nuevo post con UserID = 1 (forzado temporalmente)
            var post = new Post
            {
                UserID = 1, // Asignar temporalmente
                Title = Title,
                Description = Description,
                CreationDate = DateTime.UtcNow,
                Choices = new List<Choice>()
            };

            // Suponemos que la cantidad de textos en ChoicesTextContent coincide con la cantidad de imágenes en ChoicesImages
            for (int i = 0; i < ChoicesTextContent.Count; i++)
            {
                var choiceText = ChoicesTextContent[i];
                var imageFile = ChoicesImages.Count > i ? ChoicesImages[i] : null;

                Console.WriteLine($"Processing choice {i + 1}: Text: {choiceText}, Image: {(imageFile != null ? imageFile.FileName : "No image provided")}");

                var choice = new Choice
                {
                    TextContent = choiceText,
                    ChoiceNumber = i + 1
                };

                // Guardar la imagen si existe
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

                        // Asignar la ruta relativa, por ejemplo: "/Images/imagen.jpg"
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

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Post created with ID: {post.PostID}");
            return CreatedAtAction(nameof(GetPosts), new { id = post.PostID }, post);
        }
    }
}
