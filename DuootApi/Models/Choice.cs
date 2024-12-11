using System.Text.Json.Serialization;
namespace DuootApi.Models
{
    public class Choice
    {
        public int ChoiceID { get; set; }
        public int PostID { get; set; }
        public int ChoiceNumber { get; set; }
        public string TextContent { get; set; }
        public string ImageURL { get; set; }

        [JsonIgnore]
        public Post Post { get; set; }
        [JsonIgnore]
        public List<Vote> Votes { get; set; }
    }
}
