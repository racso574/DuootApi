using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Propiedad de Navegación
        [JsonIgnore] // Evita la serialización de PostCategories para prevenir bucles
        public List<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    }
}
