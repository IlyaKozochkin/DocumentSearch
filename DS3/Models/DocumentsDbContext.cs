using Microsoft.EntityFrameworkCore;

namespace DS3.Models
{
    public class DocumentsDbContext : DbContext
    {
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<KeyWord> KeyWords => Set<KeyWord>();
        public DbSet<TypeDoc> TypeDocs => Set<TypeDoc>();


        public DocumentsDbContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Documents.db");
        }
    }
}
