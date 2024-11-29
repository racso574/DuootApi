using Microsoft.EntityFrameworkCore;
using DuootApi.Models;

namespace DuootApi.Data
{
    public class DuootDbContext : DbContext
    {
        public DuootDbContext(DbContextOptions<DuootDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<PersonalityTrait> PersonalityTraits { get; set; }
        public DbSet<UserTrait> UserTraits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicit mapping of tables to lowercase names
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Post>().ToTable("posts");
            modelBuilder.Entity<Choice>().ToTable("choices");
            modelBuilder.Entity<Vote>().ToTable("votes");
            modelBuilder.Entity<Comment>().ToTable("comments");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<PostCategory>().ToTable("postcategories");
            modelBuilder.Entity<PersonalityTrait>().ToTable("personality_traits");
            modelBuilder.Entity<UserTrait>().ToTable("user_traits");

            // Configuring composite primary key for PostCategory
            modelBuilder.Entity<PostCategory>()
                .HasKey(pc => new { pc.PostID, pc.CategoryID });

            // Configuring composite primary key for UserTrait
            modelBuilder.Entity<UserTrait>()
                .HasKey(ut => new { ut.UserID, ut.TraitID });
        }
    }
}
