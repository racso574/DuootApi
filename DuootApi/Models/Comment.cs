
namespace DuootApi.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public int PostID { get; set; }
        public string Content { get; set; }
        public DateTime CreationDate { get; set; }

        // Navigation
        public User User { get; set; }
        public Post Post { get; set; }
    }
}
