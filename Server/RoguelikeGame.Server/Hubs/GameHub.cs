using Microsoft.AspNetCore.SignalR;

namespace RoguelikeGame.Server.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinGame(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerJoined", Context.ConnectionId);
        }

        public async Task LeaveGame(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerLeft", Context.ConnectionId);
        }

        public async Task SendGameState(string roomId, object gameState)
        {
            await Clients.OthersInGroup(roomId).SendAsync("GameStateUpdate", gameState);
        }
    }
}
