using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using MineSweeperPvP.Datas;
using MineSweeperPvP.Models;
using Microsoft.EntityFrameworkCore;

namespace MineSweeperPvP.Hubs
{
    public class GameHub : Hub
    {
        private static ConcurrentDictionary<string, List<string>> RoomConnections = new();
        private static ConcurrentDictionary<string, GameBoards> RoomBoards = new();

        private readonly ApplicationDbContext _db;

        // ✅ Inject DbContext để lưu match history
        public GameHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var kv in RoomConnections)
            {
                var list = kv.Value;
                if (list.Remove(Context.ConnectionId))
                {
                    Clients.GroupExcept(kv.Key, Context.ConnectionId).SendAsync("OpponentLeft");
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            RoomConnections.AddOrUpdate(roomId, _
                => new List<string> { Context.ConnectionId },
                (_, existing) =>
                {
                    if (!existing.Contains(Context.ConnectionId))
                        existing.Add(Context.ConnectionId);
                    return existing;
                });

            int count = RoomConnections[roomId].Count;
            await Clients.Caller.SendAsync("AssignPlayerNumber", count);
            await Clients.Group(roomId).SendAsync("PlayerJoined", count);
        }

        public Task LeaveRoom(string roomId)
        {
            Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            if (RoomConnections.TryGetValue(roomId, out var list))
            {
                list.Remove(Context.ConnectionId);
            }
            return Task.CompletedTask;
        }

        public async Task StartGame(string roomId, string player1BoardJson, string player2BoardJson)
        {
            var boards = new GameBoards
            {
                Player1Board = player1BoardJson,
                Player2Board = player2BoardJson
            };

            RoomBoards.AddOrUpdate(roomId, boards, (_, __) => boards);
            await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync("GameStarted", player1BoardJson, player2BoardJson);
        }

        public async Task CellClicked(string roomId, int playerNumber, int row, int col, bool isBomb, int adjacent)
        {
            await Clients.OthersInGroup(roomId).SendAsync("OpponentCellClicked", playerNumber, row, col, isBomb, adjacent);
        }

        public async Task FlagChanged(string roomId, int playerNumber, int row, int col, bool flagged)
        {
            await Clients.OthersInGroup(roomId).SendAsync("FlagChanged", playerNumber, row, col, flagged);
        }

        public async Task GridReset(string roomId, int playerNumber, string newBoardJson)
        {
            if (RoomBoards.TryGetValue(roomId, out var boards))
            {
                if (playerNumber == 1)
                    boards.Player1Board = newBoardJson;
                else
                    boards.Player2Board = newBoardJson;
            }

            await Clients.Group(roomId).SendAsync("PlayerGridReset", playerNumber, newBoardJson);
        }

        // ✅ Lưu kết quả khi có người thắng
        public async Task PlayerFinished(string roomId, int playerNumber)
        {
            try
            {
                // Lưu vào database
                var matchHistory = new MatchHistory
                {
                    RoomId = roomId,
                    Result = $"P{playerNumber}", // "P1" hoặc "P2"
                    MatchCreateDay = DateTime.UtcNow
                };

                _db.MatchHistories.Add(matchHistory);
                await _db.SaveChangesAsync();

                // Thông báo cho tất cả người chơi
                await Clients.Group(roomId).SendAsync("PlayerFinished", playerNumber);
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                Console.WriteLine($"Error saving match history: {ex.Message}");
                // Vẫn thông báo kết quả cho người chơi
                await Clients.Group(roomId).SendAsync("PlayerFinished", playerNumber);
            }
        }

        public Task SendMessageToRoom(string roomId, string message)
        {
            return Clients.Group(roomId).SendAsync("ReceiveMessage", message);
        }
    }

    public class GameBoards
    {
        public string Player1Board { get; set; } = "";
        public string Player2Board { get; set; } = "";
    }
}