using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class UserTrait
    {
        public int UserID { get; set; }

        public int TraitID { get; set; }

        // Propiedades de Navegación
        [JsonIgnore] // Evita la serialización de User para prevenir bucles
        public User User { get; set; }

         // Evita la serialización de PersonalityTrait para prevenir bucles
        public PersonalityTrait PersonalityTrait { get; set; }
    }
}
