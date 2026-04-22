using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Services;

namespace RoguelikeGame.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<GameHub> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IBotGameService _botGameService;

        private static readonly Dictionary<string, string> _connectionRoomMap = new();
        private static readonly Dictionary<string, string> _connectionUserMap = new();

        public GameHub(IRoomService roomService, ILogger<GameHub> logger, ApplicationDbContext dbContext, IBotGameService botGameService)
        {
            _roomService = roomService;
            _logger = logger;
            _dbContext = dbContext;
            _botGameService = botGameService;
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
                var playerUserIds = room.Players.Where(p => !p.IsBot).Select(p => p.UserId).Distinct().ToList();
                var allUserIds = playerUserIds.Append(room.HostId).Distinct().ToList();
                var userNames = await _dbContext.Users
                    .Where(u => allUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.Username);

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
                        username = p.IsBot ? (p.BotName ?? "Bot") : userNames.GetValueOrDefault(p.UserId, ""),
                        p.IsReady,
                        p.IsBot,
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
                var playerUserIds = room.Players.Where(p => !p.IsBot).Select(p => p.UserId).Distinct().ToList();
                var allUserIds = playerUserIds.Append(room.HostId).Distinct().ToList();
                var userNames = await _dbContext.Users
                    .Where(u => allUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.Username);

                await Clients.Group(roomId).SendAsync("RoomStateUpdate", new
                {
                    roomId = room.Id,
                    status = room.Status.ToString(),
                    currentPlayers = room.CurrentPlayers,
                    players = room.Players.Select(p => new
                    {
                        p.UserId,
                        username = p.IsBot ? (p.BotName ?? "Bot") : userNames.GetValueOrDefault(p.UserId, ""),
                        p.IsReady,
                        p.IsBot,
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

                var playerUserIds = room.Players.Where(p => !p.IsBot).Select(p => p.UserId).Distinct().ToList();
                var allUserIds = playerUserIds.Append(room.HostId).Distinct().ToList();
                var userNames = await _dbContext.Users
                    .Where(u => allUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.Username);

            await Clients.Group(roomId).SendAsync("GameStarting", new
            {
                roomId,
                seed = room.Seed,
                mode = room.Mode.ToString(),
                gameModeId = room.GameModeId ?? "",
                players = room.Players.Select(p => new
                {
                    p.UserId,
                    username = p.IsBot ? (p.BotName ?? "Bot") : userNames.GetValueOrDefault(p.UserId, ""),
                    p.IsBot,
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

        public async Task SendCoopCardPlay(string roomId, int playerIndex, object cardData, int targetIndex)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new HubException("未认证");

            await Clients.OthersInGroup(roomId).SendAsync("CoopCardPlayed", new
            {
                playerIndex,
                cardData,
                targetIndex,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendCoopTurnEnd(string roomId, int playerIndex)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new HubException("未认证");

            await Clients.OthersInGroup(roomId).SendAsync("CoopTurnEnded", new
            {
                playerIndex,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendRacePosition(string roomId, string racerId, double x, double y, string form)
        {
            await Clients.OthersInGroup(roomId).SendAsync("RacePositionUpdate", new
            {
                racerId,
                x,
                y,
                form,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendRaceCheckpoint(string roomId, string racerId, string checkpointId)
        {
            await Clients.Group(roomId).SendAsync("RaceCheckpointReached", new
            {
                racerId,
                checkpointId,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendRaceFinish(string roomId, string racerId, double finishTime)
        {
            await Clients.Group(roomId).SendAsync("RaceFinished", new
            {
                racerId,
                finishTime,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendCoopPosition(string roomId, double x, double y, string form)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new HubException("未认证");

            await Clients.OthersInGroup(roomId).SendAsync("CoopPositionUpdate", new
            {
                userId,
                x,
                y,
                form,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendCoopSwitch(string roomId, string switchId, bool activated)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new HubException("未认证");

            await Clients.OthersInGroup(roomId).SendAsync("CoopSwitchUpdate", new
            {
                userId,
                switchId,
                activated,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendCoopPuzzleSolved(string roomId, string puzzleId)
        {
            await Clients.Group(roomId).SendAsync("CoopPuzzleSolved", new
            {
                puzzleId,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendCoopPlayerDied(string roomId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new HubException("未认证");

            await Clients.OthersInGroup(roomId).SendAsync("CoopPlayerDied", new
            {
                userId,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendCoopPlayerRevived(string roomId)
        {
            await Clients.Group(roomId).SendAsync("CoopPlayerRevived", new
            {
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendLevelCompleted(string roomId, string levelId, string userId, object resultData)
        {
            await Clients.Group(roomId).SendAsync("LevelCompleted", new
            {
                roomId,
                levelId,
                userId,
                resultData,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendGameEnded(string roomId, object gameResult)
        {
            await Clients.Group(roomId).SendAsync("GameEnded", new
            {
                roomId,
                gameResult,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task UpdateLightShadowGameState(string roomId, string botUserId, object gameState)
        {
            try
            {
                var stateDict = new Dictionary<string, object>();
                if (gameState is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var innerJson = jsonElement.GetString();
                        if (!string.IsNullOrEmpty(innerJson))
                        {
                            jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(innerJson);
                        }
                    }
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var prop in jsonElement.EnumerateObject())
                        {
                            switch (prop.Value.ValueKind)
                            {
                                case System.Text.Json.JsonValueKind.Number:
                                    if (prop.Value.TryGetInt32(out int intVal))
                                        stateDict[prop.Name] = intVal;
                                    else if (prop.Value.TryGetDouble(out double doubleVal))
                                        stateDict[prop.Name] = doubleVal;
                                    break;
                                case System.Text.Json.JsonValueKind.True:
                                    stateDict[prop.Name] = true;
                                    break;
                                case System.Text.Json.JsonValueKind.False:
                                    stateDict[prop.Name] = false;
                                    break;
                                case System.Text.Json.JsonValueKind.String:
                                    stateDict[prop.Name] = prop.Value.GetString() ?? "";
                                    break;
                                case System.Text.Json.JsonValueKind.Object:
                                case System.Text.Json.JsonValueKind.Array:
                                    stateDict[prop.Name] = prop.Value;
                                    break;
                            }
                        }
                    }
                }
                else if (gameState is string gameStateStr)
                {
                    var parsed = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(gameStateStr);
                    foreach (var prop in parsed.EnumerateObject())
                    {
                        switch (prop.Value.ValueKind)
                        {
                            case System.Text.Json.JsonValueKind.Number:
                                if (prop.Value.TryGetInt32(out int intVal2))
                                    stateDict[prop.Name] = intVal2;
                                else if (prop.Value.TryGetDouble(out double doubleVal2))
                                    stateDict[prop.Name] = doubleVal2;
                                break;
                            case System.Text.Json.JsonValueKind.True:
                                stateDict[prop.Name] = true;
                                break;
                            case System.Text.Json.JsonValueKind.False:
                                stateDict[prop.Name] = false;
                                break;
                            case System.Text.Json.JsonValueKind.String:
                                stateDict[prop.Name] = prop.Value.GetString() ?? "";
                                break;
                            case System.Text.Json.JsonValueKind.Object:
                            case System.Text.Json.JsonValueKind.Array:
                                stateDict[prop.Name] = prop.Value;
                                break;
                        }
                    }
                }
                else if (gameState is Dictionary<string, object> dict)
                {
                    stateDict = dict;
                }

                _logger.LogDebug("Bot game state updated for {BotUserId} in room {RoomId}, keys: {Keys}",
                    botUserId, roomId, string.Join(",", stateDict.Keys));
                _botGameService.UpdateBotGameState(roomId, botUserId, stateDict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update bot game state for {BotUserId} in room {RoomId}", botUserId, roomId);
            }
        }
    }
}
