
namespace DuootApi.Models
{
    public class PostCategory
    {
        public int PostID { get; set; }
        public int CategoryID { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public Category Category { get; set; }
    }
}
