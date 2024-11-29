
namespace DuootApi.Models
{
    public class Vote
    {
        public int VoteID { get; set; }
        public int UserID { get; set; }
        public int PostID { get; set; }
        public int ChoiceID { get; set; }
        public DateTime VoteDate { get; set; }

        // Navigation
        public User User { get; set; }
        public Post Post { get; set; }
        public Choice Choice { get; set; }
    }
}
