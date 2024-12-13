using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class Post
    {
        public int PostID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public DateTime CreationDate { get; set; }

        [Required]
        public string Description { get; set; }

        // Propiedades de Navegación
        [JsonIgnore] // Evita la serialización de User para prevenir bucles
        public User User { get; set; }

         // Evita la serialización de Choices para optimizar la respuesta
        public List<Choice> Choices { get; set; } = new List<Choice>();

        // Evita la serialización de PostCategories para prevenir bucles
        public List<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    }
}
