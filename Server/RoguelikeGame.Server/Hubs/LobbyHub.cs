using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using RoguelikeGame.Server.Services;

namespace RoguelikeGame.Server.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly IAuthService _authService;
        private readonly IRoomService _roomService;
        private readonly ILogger<LobbyHub> _logger;

        private static readonly Dictionary<string, string> _userConnections = new();

        public LobbyHub(IAuthService authService, IRoomService roomService, ILogger<LobbyHub> logger)
        {
            _authService = authService;
            _roomService = roomService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                _userConnections[Context.ConnectionId] = userId;

                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

                _logger.LogInformation("用户连接到大厅: {UserId}, ConnectionId: {ConnectionId}",
                    userId, Context.ConnectionId);

                await Clients.Caller.SendAsync("Connected", new
                {
                    message = "欢迎来到大厅！",
                    connectionId = Context.ConnectionId
                });

                await Clients.Others.SendAsync("UserOnline", new { userId });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null && _userConnections.ContainsKey(Context.ConnectionId))
            {
                _userConnections.Remove(Context.ConnectionId);

                _logger.LogInformation("用户断开大厅连接: {UserId}", userId);

                await Clients.Group($"User_{userId}").SendAsync("ForceDisconnect",
                    new { reason = "从其他设备登录" });

                await Clients.Others.SendAsync("UserOffline", new { userId });
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string targetUserId, string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) throw new HubException("未认证");

            _logger.LogDebug("大厅消息: {SenderId} -> {TargetUserId}: {Message}",
                senderId, targetUserId, message);

            await Clients.Group($"User_{targetUserId}").SendAsync("ReceiveMessage", new
            {
                senderId,
                message,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task BroadcastMessage(string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) throw new HubException("未认证");

            await Clients.All.SendAsync("ReceiveBroadcast", new
            {
                senderId,
                message,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyRoomCreated(string roomId)
        {
            await Clients.All.SendAsync("RoomCreated", new { roomId });
        }

        public async Task NotifyRoomUpdated(string roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room != null)
            {
                await Clients.All.SendAsync("RoomUpdated", room);
            }
        }
    }
}
