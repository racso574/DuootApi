
namespace DuootApi.Models
{
    public class UserTrait
    {
        public int UserID { get; set; }
        public int TraitID { get; set; }

        // Navigation
        public User User { get; set; }
        public PersonalityTrait PersonalityTrait { get; set; }
    }
}
