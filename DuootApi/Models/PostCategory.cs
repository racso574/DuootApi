using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class PostCategory
    {
        public int PostID { get; set; }

        public int CategoryID { get; set; }

        // Propiedades de Navegación
        [JsonIgnore] // Evita la serialización de Post para prevenir bucles
        public Post Post { get; set; }

        [JsonIgnore] // Evita la serialización de Category para prevenir bucles
        public Category Category { get; set; }
    }
}
