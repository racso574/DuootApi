
namespace DuootApi.Models
{
    public class Post
    {
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }

        // Navigation
        public User User { get; set; }
        public List<Choice> Choices { get; set; }
        public List<Vote> Votes { get; set; }
        public List<Comment> Comments { get; set; }

        public List<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    }
}
