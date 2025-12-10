using Microsoft.EntityFrameworkCore;
using MineSweeperPvP.Models;

namespace MineSweeperPvP.Datas
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<MatchHistory> MatchHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Room>().HasKey(r => r.RoomId);
            modelBuilder.Entity<MatchHistory>().HasKey(m => m.Id);
        }
    }
}
