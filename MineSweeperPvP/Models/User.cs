using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MineSweeperPvP.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Đảm bảo auto-increment
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
    }
}
