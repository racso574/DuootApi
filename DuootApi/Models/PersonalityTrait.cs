using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class PersonalityTrait
    {
        [Key]
        public int TraitID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        // Propiedad de Navegación
        [JsonIgnore] // Evita la serialización de UserTraits para prevenir bucles
        public List<UserTrait> UserTraits { get; set; } = new List<UserTrait>();
    }
}
