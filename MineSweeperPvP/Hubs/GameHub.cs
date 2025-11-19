using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace MineSweeperPvP.Hubs
{
    public class GameHub : Hub
    {
        // Map roomId -> list of connectionIds (max 2)
        private static ConcurrentDictionary<string, List<string>> RoomConnections = new();

        // Store game boards for each room: roomId -> (player1Board, player2Board)
        private static ConcurrentDictionary<string, GameBoards> RoomBoards = new();

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var kv in RoomConnections)
            {
                var list = kv.Value;
                if (list.Remove(Context.ConnectionId))
                {
                    // Notify OTHER players only (exclude the disconnected one)
                    Clients.GroupExcept(kv.Key, Context.ConnectionId).SendAsync("OpponentLeft");
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            RoomConnections.AddOrUpdate(roomId, _ => new List<string> { Context.ConnectionId }, (_, existing) =>
            {
                if (!existing.Contains(Context.ConnectionId)) existing.Add(Context.ConnectionId);
                return existing;
            });

            await Clients.Group(roomId).SendAsync("PlayerJoined", RoomConnections[roomId].Count);
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

        // When game starts, store the boards
        public async Task StartGame(string roomId, string player1BoardJson, string player2BoardJson)
        {
            var boards = new GameBoards
            {
                Player1Board = player1BoardJson,
                Player2Board = player2BoardJson
            };

            RoomBoards.AddOrUpdate(roomId, boards, (_, __) => boards);

            // Send boards to both players
            await Clients.Group(roomId).SendAsync("GameStarted", player1BoardJson, player2BoardJson);
        }

        // When a player clicks a cell, broadcast to the other player
        public async Task CellClicked(string roomId, int playerNumber, int row, int col, bool isBomb, int adjacent)
        {
            await Clients.OthersInGroup(roomId).SendAsync("OpponentCellClicked", playerNumber, row, col, isBomb, adjacent);
        }

        // When a player's grid needs to be reset (hit a bomb)
        public async Task GridReset(string roomId, int playerNumber, string newBoardJson)
        {
            // Update stored board
            if (RoomBoards.TryGetValue(roomId, out var boards))
            {
                if (playerNumber == 1)
                    boards.Player1Board = newBoardJson;
                else
                    boards.Player2Board = newBoardJson;
            }

            await Clients.Group(roomId).SendAsync("PlayerGridReset", playerNumber, newBoardJson);
        }

        // When a player finishes their grid (revealed all safe cells)
        public async Task PlayerFinished(string roomId, int playerNumber)
        {
            await Clients.Group(roomId).SendAsync("PlayerFinished", playerNumber);
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