using System;
using System.Text.Json.Serialization;
namespace DuootApi.Models
{
    public class Post
    {
       public int PostID { get; set; }
    public int UserID { get; set; }
    public string Title { get; set; }
    public DateTime CreationDate { get; set; }
    public string Description { get; set; }

    // Navigation properties
    [JsonIgnore]
    public User User { get; set; }
    [JsonIgnore]
    public List<Choice> Choices { get; set; }
    [JsonIgnore]
    public List<PostCategory> PostCategories { get; set; }
    }
}
