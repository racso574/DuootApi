using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using DuootApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DuootDbContext _context;

        public CategoriesController(DuootDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories - Obtener todas las categorías
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}
