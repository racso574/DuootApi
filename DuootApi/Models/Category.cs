namespace DuootApi.Models{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }

        // Navigation property
        public List<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    }
}