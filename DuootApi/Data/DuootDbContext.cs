using Microsoft.EntityFrameworkCore;
using DuootApi.Models;
using System.Text;

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

    // Aplicar convención global para convertir nombres a minúsculas sin guiones
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        // Configurar el nombre de las tablas en minúsculas
        entity.SetTableName(ToLowerCase(entity.GetTableName()));

        // Configurar las columnas en minúsculas
        foreach (var property in entity.GetProperties())
        {
            property.SetColumnName(ToLowerCase(property.Name));
        }

        // Configurar las claves primarias y foráneas en minúsculas
        foreach (var key in entity.GetKeys())
        {
            key.SetName(ToLowerCase(key.GetName()));
        }

        foreach (var foreignKey in entity.GetForeignKeys())
        {
            foreignKey.SetConstraintName(ToLowerCase(foreignKey.GetConstraintName()));
        }

        foreach (var index in entity.GetIndexes())
        {
            index.SetDatabaseName(ToLowerCase(index.GetDatabaseName()));
        }
    }

    // Configurar relaciones específicas o claves compuestas
    modelBuilder.Entity<PostCategory>()
        .HasKey(pc => new { pc.PostID, pc.CategoryID });

    modelBuilder.Entity<UserTrait>()
        .HasKey(ut => new { ut.UserID, ut.TraitID });

    modelBuilder.Entity<UserTrait>()
        .HasOne(ut => ut.User)
        .WithMany(u => u.UserTraits)
        .HasForeignKey(ut => ut.UserID);

    modelBuilder.Entity<UserTrait>()
        .HasOne(ut => ut.PersonalityTrait)
        .WithMany(pt => pt.UserTraits)
        .HasForeignKey(ut => ut.TraitID);
}

// Método auxiliar para convertir nombres a minúsculas
private static string ToLowerCase(string input)
{
    return input?.ToLower();
}

    }
}
