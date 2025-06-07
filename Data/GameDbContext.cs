using Microsoft.EntityFrameworkCore;

namespace MatrixGame.Data
{
    public class GameDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.db");
            options.UseSqlite($"Data Source={path}");

            Console.WriteLine("DB Path: " + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.db"));
        }
    }
}
