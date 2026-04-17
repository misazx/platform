using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using RoguelikeGame.Server.Services;

namespace RoguelikeGame.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<GameHub> _logger;

        private static readonly Dictionary<string, string> _connectionRoomMap = new();
        private static readonly Dictionary<string, string> _connectionUserMap = new();

        public GameHub(IRoomService roomService, ILogger<GameHub> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (userId != null)
            {
                _connectionUserMap[Context.ConnectionId] = userId;
                _logger.LogInformation("[GameHub] 用户连接: {Username} ({UserId}), ConnectionId: {ConnectionId}",
                    username, userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectionRoomMap.TryGetValue(Context.ConnectionId, out var roomId))
            {
                _connectionRoomMap.Remove(Context.ConnectionId);

                var userId = _connectionUserMap.GetValueOrDefault(Context.ConnectionId);
                var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                _logger.LogInformation("[GameHub] 用户断开: {Username} 从房间 {RoomId}", username, roomId);

                await Clients.Group(roomId).SendAsync("PlayerLeftRoom", new
                {
                    playerId = userId,
                    playerName = username,
                    reason = "disconnect"
                });

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            }

            _connectionUserMap.Remove(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (userId == null) throw new HubException("未认证");

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            _connectionRoomMap[Context.ConnectionId] = roomId;

            _logger.LogInformation("[GameHub] {Username} 加入房间 {RoomId}", username, roomId);

            await Clients.Group(roomId).SendAsync("PlayerJoinedRoom", new
            {
                playerId = userId,
                playerName = username,
                timestamp = DateTime.UtcNow
            });

            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room != null)
            {
                await Clients.Caller.SendAsync("RoomStateUpdate", new
                {
                    roomId = room.Id,
                    roomName = room.Name,
                    hostId = room.HostId,
                    status = room.Status.ToString(),
                    mode = room.Mode.ToString(),
                    maxPlayers = room.MaxPlayers,
                    currentPlayers = room.CurrentPlayers,
                    players = room.Players.Select(p => new
                    {
                        p.UserId,
                        username = p.User?.Username ?? "",
                        p.IsReady,
                        isHost = p.UserId == room.HostId
                    })
                });
            }
        }

        public async Task LeaveRoom(string roomId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (userId == null) throw new HubException("未认证");

            _connectionRoomMap.Remove(Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

            _logger.LogInformation("[GameHub] {Username} 离开房间 {RoomId}", username, roomId);

            await Clients.Group(roomId).SendAsync("PlayerLeftRoom", new
            {
                playerId = userId,
                playerName = username,
                reason = "leave"
            });
        }

        public async Task SendRoomChat(string roomId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (userId == null) throw new HubException("未认证");
            if (string.IsNullOrWhiteSpace(message)) return;
            if (message.Length > 500) message = message.Substring(0, 500);

            _logger.LogDebug("[GameHub] 房间聊天 [{RoomId}] {Username}: {Message}", roomId, username, message);

            await Clients.Group(roomId).SendAsync("RoomChatMessage", new
            {
                senderId = userId,
                senderName = username,
                message,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyReadyChanged(string roomId, bool isReady)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (userId == null) throw new HubException("未认证");

            _logger.LogInformation("[GameHub] {Username} 准备状态变更: {IsReady}", username, isReady);

            await Clients.Group(roomId).SendAsync("PlayerReadyChanged", new
            {
                playerId = userId,
                playerName = username,
                isReady,
                timestamp = DateTime.UtcNow
            });

            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room != null)
            {
                await Clients.Group(roomId).SendAsync("RoomStateUpdate", new
                {
                    roomId = room.Id,
                    status = room.Status.ToString(),
                    currentPlayers = room.CurrentPlayers,
                    players = room.Players.Select(p => new
                    {
                        p.UserId,
                        username = p.User?.Username ?? "",
                        p.IsReady,
                        isHost = p.UserId == room.HostId
                    })
                });
            }
        }

        public async Task NotifyGameStarting(string roomId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new HubException("未认证");

            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null || room.HostId != userId) throw new HubException("仅房主可开始游戏");

            _logger.LogInformation("[GameHub] 游戏 [{RoomId}] 由 {UserId} 启动", roomId, userId);

            await Clients.Group(roomId).SendAsync("GameStarting", new
            {
                roomId,
                seed = room.Seed,
                mode = room.Mode.ToString(),
                players = room.Players.Select(p => new
                {
                    p.UserId,
                    username = p.User?.Username ?? "",
                    isHost = p.UserId == room.HostId
                }),
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendGameState(string roomId, object gameState)
        {
            await Clients.OthersInGroup(roomId).SendAsync("GameStateUpdate", gameState);
        }

        public async Task NotifyBotAdded(string roomId, string botName)
        {
            await Clients.Group(roomId).SendAsync("BotAdded", new
            {
                botName,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyBotRemoved(string roomId, string botName)
        {
            await Clients.Group(roomId).SendAsync("BotRemoved", new
            {
                botName,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
