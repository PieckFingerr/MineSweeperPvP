using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;


namespace MineSweeperPvP.Hubs
{
    public class GameHub : Hub
    {
        // Map roomId -> list of connectionIds (max 2)
        private static ConcurrentDictionary<string, List<string>> RoomConnections = new();

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var kv in RoomConnections)
            {
                var list = kv.Value;
                if (list.Remove(Context.ConnectionId))
                {
                    // notify other player
                    Clients.Group(kv.Key).SendAsync("OpponentLeft");
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

        // When a player finishes their grid (revealed all safe cells)
        public async Task PlayerFinished(string roomId, int playerNumber)
        {
            // notify group
            await Clients.Group(roomId).SendAsync("PlayerFinished", playerNumber);
        }

        // When a player clicks a bomb -> reset that player's grid only
        public async Task BombClicked(string roomId, string connectionId)
        {
            await Clients.Client(connectionId).SendAsync("ResetGrid");
        }

        // informational messages like sync timer etc
        public Task SendMessageToRoom(string roomId, string message)
        {
            return Clients.Group(roomId).SendAsync("ReceiveMessage", message);
        }
    }
}
