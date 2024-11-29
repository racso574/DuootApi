
namespace DuootApi.Models
{
    public class PersonalityTrait
    {
        public int TraitID { get; set; }
        public string Description { get; set; }

        // Navigation
        public List<UserTrait> UserTraits { get; set; } = new List<UserTrait>();
    }
}
