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
    }
}
