using System.ComponentModel.DataAnnotations;

namespace DuootApi.Models
{
    public class PersonalityTrait
    {
        [Key] // Clave primaria
        public int TraitID { get; set; }

        [Required]
        [StringLength(100)] // Agrega restricciones como en otros modelos
        public string Description { get; set; }

        // Navigation
        public List<UserTrait> UserTraits { get; set; } = new List<UserTrait>();
    }
}
