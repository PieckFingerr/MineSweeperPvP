namespace MineSweeperPvP.Models
{
    public class MatchHistory
    {
        public int Id { get; set; }
        public string? RoomId { get; set; }
        public string? Result { get; set; } // "P1" or "P2"
        public DateTime MatchCreateDay { get; set; } = DateTime.UtcNow;
    }
}
