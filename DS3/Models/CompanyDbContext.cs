using Microsoft.EntityFrameworkCore;

namespace DS3.Models
{
    public class CompanyDbContext : DbContext
    {
        public DbSet<CompanyDocument> Documents => Set<CompanyDocument>();

        public CompanyDbContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=CompanyDocuments.db");
        }
    }
}
