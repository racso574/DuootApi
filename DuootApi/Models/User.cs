using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(255)]
        public string? ProfileImage { get; set; }

        // Propiedades de Navegación
        [JsonIgnore] // Evita la serialización de Posts para optimizar la respuesta
        public List<Post> Posts { get; set; } = new List<Post>();

        [JsonIgnore] // Evita la serialización de Votes para optimizar la respuesta
        public List<Vote> Votes { get; set; } = new List<Vote>();

        [JsonIgnore] // Evita la serialización de Comments para optimizar la respuesta
        public List<Comment> Comments { get; set; } = new List<Comment>();

        
        public List<UserTrait> UserTraits { get; set; } = new List<UserTrait>();
    }
}
