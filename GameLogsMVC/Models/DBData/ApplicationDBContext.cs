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
        public DbSet<Player> Player { get; set; }
        public DbSet<PlayerGame> PlayerGame { get; set; }
        public DbSet<Team> Team { get; set; }
        public DbSet<UserFollow> UserFollow { get; set; }
        public DbSet<UserBadge> UserBadge { get; set; }
        public DbSet<Badge> Badge { get; set; }
    }
}
