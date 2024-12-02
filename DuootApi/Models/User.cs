
using System.ComponentModel.DataAnnotations;

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

        // Navigation - Initialized to avoid NullReferenceException
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Vote> Votes { get; set; } = new List<Vote>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<UserTrait> UserTraits { get; set; } = new List<UserTrait>();
    }
}
