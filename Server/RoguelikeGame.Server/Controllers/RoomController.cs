using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using RoguelikeGame.Server.Models;
using RoguelikeGame.Server.Services;

namespace RoguelikeGame.Server.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IAuthService _authService;
        private readonly ILogger<RoomController> _logger;

        public RoomController(IRoomService roomService, IAuthService authService, ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var room = await _roomService.CreateRoomAsync(
                    userId,
                    request.Name,
                    request.Mode,
                    request.MaxPlayers,
                    request.Password
                );

                _logger.LogInformation("房间已创建: {RoomId} by {UserId}", room.Id, userId);

                return Ok(new
                {
                    success = true,
                    roomId = room.Id,
                    seed = room.Seed,
                    message = "房间创建成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建房间失败");
                return StatusCode(500, new { success = false, message = "创建房间失败" });
            }
        }

        [HttpPost("{roomId}/join")]
        public async Task<IActionResult> JoinRoom(string roomId, [FromBody] JoinRoomRequest? request = null)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var (success, room, message) = await _roomService.JoinRoomAsync(roomId, userId);

                if (success && room != null)
                {
                    _logger.LogInformation("玩家加入房间: {UserId} -> {RoomId}", userId, roomId);

                    var roomData = new
                    {
                        room.Id,
                        room.Name,
                        room.HostId,
                        hostName = room.Host?.Username ?? "",
                        room.Status,
                        room.Mode,
                        room.MaxPlayers,
                        room.CurrentPlayers,
                        room.HasPassword,
                        room.Seed,
                        room.CreatedAt,
                        players = room.Players.Select(p => new
                        {
                            p.Id,
                            p.UserId,
                            username = p.User?.Username ?? "",
                            p.IsReady,
                            p.CharacterId,
                            p.Score,
                            p.JoinedAt
                        })
                    };

                    return Ok(new { success = true, room = roomData, message });
                }
                else
                {
                    return BadRequest(new { success = false, message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加入房间失败");
                return StatusCode(500, new { success = false, message = "加入房间失败" });
            }
        }

        [HttpPost("{roomId}/leave")]
        public async Task<IActionResult> LeaveRoom(string roomId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _roomService.LeaveRoomAsync(roomId, userId);

            if (success)
            {
                _logger.LogInformation("玩家离开房间: {UserId} <- {RoomId}", userId, roomId);
                return Ok(new { success = true, message = "已离开房间" });
            }
            else
            {
                return BadRequest(new { success = false, message = "离开房间失败" });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var rooms = await _roomService.GetPublicRoomsAsync(page, pageSize);

                var roomList = rooms.Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.HostId,
                    hostName = r.Host?.Username ?? "",
                    r.Status,
                    r.Mode,
                    r.MaxPlayers,
                    r.CurrentPlayers,
                    r.HasPassword,
                    r.Seed,
                    r.CreatedAt,
                    players = r.Players.Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        username = p.User?.Username ?? "",
                        p.IsReady,
                        p.CharacterId,
                        p.Score,
                        p.JoinedAt
                    })
                });

                return Ok(new
                {
                    success = true,
                    rooms = roomList,
                    page,
                    pageSize,
                    total = rooms.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取房间列表失败");
                return StatusCode(500, new { success = false, message = "获取房间列表失败" });
            }
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoom(string roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);

            if (room == null)
            {
                return NotFound(new { success = false, message = "房间不存在" });
            }

            var roomData = new
            {
                room.Id,
                room.Name,
                room.HostId,
                hostName = room.Host?.Username ?? "",
                room.Status,
                room.Mode,
                room.MaxPlayers,
                room.CurrentPlayers,
                room.HasPassword,
                room.Seed,
                room.CreatedAt,
                players = room.Players.Select(p => new
                {
                    p.Id,
                    p.UserId,
                    username = p.User?.Username ?? "",
                    p.IsReady,
                    p.CharacterId,
                    p.Score,
                    p.JoinedAt
                })
            };

            return Ok(new { success = true, room = roomData });
        }

        [HttpPost("{roomId}/ready")]
        public async Task<IActionResult> SetReady(string roomId, [FromBody] ReadyRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _roomService.SetPlayerReadyAsync(roomId, userId, request.IsReady);

            if (success)
            {
                return Ok(new { success = true, message = request.IsReady ? "已准备就绪" : "取消准备" });
            }
            else
            {
                return BadRequest(new { success = false, message = "操作失败" });
            }
        }

        [HttpPost("{roomId}/start")]
        public async Task<IActionResult> StartGame(string roomId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _roomService.StartGameAsync(roomId, userId);

            if (success)
            {
                _logger.LogInformation("游戏开始: {RoomId}", roomId);
                return Ok(new { success = true, message = "游戏已开始" });
            }
            else
            {
                return BadRequest(new { success = false, message = "无法开始游戏（需要房主权限或所有玩家未就绪）" });
            }
        }

        [HttpPost("{roomId}/add-bot")]
        public async Task<IActionResult> AddBot(string roomId, [FromBody] AddBotRequest? request = null)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var difficulty = request?.Difficulty ?? "Normal";
            var (success, botPlayer, message) = await _roomService.AddBotAsync(roomId, userId, difficulty);

            if (success && botPlayer != null)
            {
                _logger.LogInformation("机器人加入房间: {BotName} -> {RoomId}", botPlayer.BotName, roomId);
                return Ok(new
                {
                    success = true,
                    bot = new
                    {
                        botPlayer.Id,
                        botPlayer.UserId,
                        botName = botPlayer.BotName,
                        botDifficulty = botPlayer.BotDifficulty,
                        botPlayer.IsReady,
                        botPlayer.IsBot
                    },
                    message
                });
            }
            else
            {
                return BadRequest(new { success = false, message });
            }
        }

        [HttpPost("{roomId}/remove-bot")]
        public async Task<IActionResult> RemoveBot(string roomId, [FromBody] RemoveBotRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var (success, message) = await _roomService.RemoveBotAsync(roomId, userId, request.BotId);

            if (success)
            {
                _logger.LogInformation("机器人移出房间: {BotId} <- {RoomId}", request.BotId, roomId);
                return Ok(new { success = true, message });
            }
            else
            {
                return BadRequest(new { success = false, message });
            }
        }
    }

    public class CreateRoomRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameMode Mode { get; set; } = GameMode.PvP;

        [Range(2, 4)]
        public int MaxPlayers { get; set; } = 4;

        public string? Password { get; set; }
    }

    public class JoinRoomRequest
    {
        public string? Password { get; set; }
    }

    public class ReadyRequest
    {
        public bool IsReady { get; set; } = true;
    }

    public class AddBotRequest
    {
        public string? Difficulty { get; set; }
    }

    public class RemoveBotRequest
    {
        [Required]
        public string BotId { get; set; } = "";
    }
}
