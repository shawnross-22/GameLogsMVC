using Microsoft.EntityFrameworkCore;

namespace GameLogsMVC.Models.DBData
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> User { get; set; }
        public DbSet<Game> Game { get; set; }
        public DbSet<UserGame> UserGame { get; set; }
    }
}
