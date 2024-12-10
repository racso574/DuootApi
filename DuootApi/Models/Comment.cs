using System;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public int PostID { get; set; }
        public string Content { get; set; }
        public DateTime CreationDate { get; set; }

        // Propiedades de Navegación Opcionales
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public Post? Post { get; set; }
    }
}
