using System;
using DS3.Models;
using Microsoft.EntityFrameworkCore;

namespace DS3.Models
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        public UserDbContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=UsersData.db");
        }
    }
}
