using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class Comment
    {
        public int CommentID { get; set; }

        public int UserID { get; set; }

        public int PostID { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public DateTime CreationDate { get; set; }

        // Propiedades de Navegación
        [JsonIgnore] // Evita la serialización de User para prevenir bucles
        public User? User { get; set; }

        [JsonIgnore] // Evita la serialización de Post para prevenir bucles
        public Post? Post { get; set; }
    }
}
