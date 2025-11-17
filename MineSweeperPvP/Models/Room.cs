namespace MineSweeperPvP.Models
{
    public class Room
    {
        public string? RoomId { get; set; }
        public int? Player1Id { get; set; }
        public int? Player2Id { get; set; }
        public DateTime CreateDay { get; set; } = DateTime.UtcNow;
    }
}
