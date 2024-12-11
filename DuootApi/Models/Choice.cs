using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class Choice
    {
        public int ChoiceID { get; set; }

        public int PostID { get; set; }

        public int ChoiceNumber { get; set; }

        [Required]
        [StringLength(500)]
        public string TextContent { get; set; }

        [StringLength(2048)]
        public string ImageURL { get; set; }

        // Propiedades de Navegación
        [JsonIgnore] // Evita la serialización de Post para prevenir bucles
        public Post Post { get; set; }

        [JsonIgnore] // Evita la serialización de Votes para optimizar la respuesta
        public List<Vote> Votes { get; set; } = new List<Vote>();
    }
}
