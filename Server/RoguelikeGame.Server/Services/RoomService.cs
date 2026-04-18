using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Services
{
    public interface IRoomService
    {
        Task<Room> CreateRoomAsync(string hostId, string name, GameMode mode, int maxPlayers = 4, string? password = null);
        Task<(bool Success, Room? Room, string Message)> JoinRoomAsync(string roomId, string userId, string? password = null);
        Task<bool> LeaveRoomAsync(string roomId, string userId);
        Task<List<Room>> GetPublicRoomsAsync(int page = 1, int pageSize = 20);
        Task<Room?> GetRoomByIdAsync(string roomId);
        Task<bool> SetPlayerReadyAsync(string roomId, string userId, bool isReady);
        Task<bool> StartGameAsync(string roomId, string hostId);
        Task<bool> EndGameAsync(string roomId, bool victory);
        Task CleanupExpiredRoomsAsync();
        Task<(bool Success, RoomPlayer? BotPlayer, string Message)> AddBotAsync(string roomId, string hostId, string difficulty = "Normal");
        Task<(bool Success, string Message)> RemoveBotAsync(string roomId, string hostId, string botId);
    }

    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Room> CreateRoomAsync(string hostId, string name, GameMode mode, int maxPlayers = 4, string? password = null)
        {
            var room = new Room
            {
                Name = name,
                HostId = hostId,
                Mode = mode,
                MaxPlayers = maxPlayers,
                HasPassword = !string.IsNullOrEmpty(password),
                PasswordHash = password != null ? BCrypt.Net.BCrypt.HashPassword(password) : null,
                Seed = Guid.NewGuid().ToString()
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            await JoinRoomAsync(room.Id, hostId);

            return room;
        }

        public async Task<(bool Success, Room? Room, string Message)> JoinRoomAsync(string roomId, string userId, string? password = null)
        {
            var room = await _context.Rooms
                .Include(r => r.Players)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return (false, null, "房间不存在");
            }

            if (room.Status != RoomStatus.Waiting)
            {
                return (false, null, "房间已关闭或游戏中");
            }

            if (room.CurrentPlayers >= room.MaxPlayers)
            {
                return (false, null, "房间已满");
            }

            if (room.HasPassword && !string.IsNullOrEmpty(room.PasswordHash))
            {
                if (string.IsNullOrEmpty(password) || !BCrypt.Net.BCrypt.Verify(password, room.PasswordHash))
                {
                    return (false, null, "房间密码错误");
                }
            }

            var alreadyInRoom = room.Players.Any(p => p.UserId == userId);
            if (alreadyInRoom)
            {
                return (true, room, "已在房间中");
            }

            var player = new RoomPlayer
            {
                RoomId = roomId,
                UserId = userId,
                IsReady = room.HostId == userId
            };

            _context.RoomPlayers.Add(player);
            room.CurrentPlayers++;

            if (room.CurrentPlayers >= room.MaxPlayers)
            {
                room.Status = RoomStatus.Full;
            }

            await _context.SaveChangesAsync();

            return (true, room, "加入房间成功");
        }

        public async Task<bool> LeaveRoomAsync(string roomId, string userId)
        {
            var player = await _context.RoomPlayers
                .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);

            if (player == null) return false;

            var room = await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return false;

            _context.RoomPlayers.Remove(player);
            room.CurrentPlayers--;

            if (room.HostId == userId && room.Players.Count() > 1)
            {
                var newHost = room.Players.FirstOrDefault(p => p.UserId != userId);
                if (newHost != null)
                {
                    room.HostId = newHost.UserId;
                }
            }
            else if (room.Players.Count() == 0 || room.HostId == userId)
            {
                room.Status = RoomStatus.Finished;
            }
            else if (room.Status == RoomStatus.Full)
            {
                room.Status = RoomStatus.Waiting;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Room>> GetPublicRoomsAsync(int page = 1, int pageSize = 20)
        {
            return await _context.Rooms
                .Include(r => r.Host)
				.Include(r => r.Players)
					.ThenInclude(p => p.User)
                .Where(r => r.Status == RoomStatus.Waiting && !r.HasPassword)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(string roomId)
        {
            return await _context.Rooms
                .Include(r => r.Players)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }

        public async Task<bool> SetPlayerReadyAsync(string roomId, string userId, bool isReady)
        {
            var player = await _context.RoomPlayers
                .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);

            if (player == null) return false;

            player.IsReady = isReady;
            await _context.SaveChangesAsync();

            var room = await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room != null && room.Players.All(p => p.IsReady) && room.Players.Count >= 2)
            {
                room.Status = RoomStatus.Ready;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> StartGameAsync(string roomId, string hostId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            if (room.HostId != hostId) return false;
			if (room.Status != RoomStatus.Ready && room.Status != RoomStatus.Full) return false;

            room.Status = RoomStatus.Playing;
            room.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EndGameAsync(string roomId, bool victory)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            room.Status = RoomStatus.Finished;
            room.FinishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CleanupExpiredRoomsAsync()
        {
            var expiredRooms = await _context.Rooms
                .Where(r => r.Status == RoomStatus.Waiting &&
                           r.CreatedAt < DateTime.UtcNow.AddHours(-1))
                .ToListAsync();

            foreach (var room in expiredRooms)
            {
                room.Status = RoomStatus.Finished;
            }

            await _context.SaveChangesAsync();
        }

        private static readonly string[] BOT_NAMES = {
            "AI-Alpha", "AI-Bravo", "AI-Charlie", "AI-Delta",
            "AI-Echo", "AI-Foxtrot", "AI-Golf", "AI-Hotel"
        };

        private static int _botNameIndex;

        public async Task<(bool Success, RoomPlayer? BotPlayer, string Message)> AddBotAsync(string roomId, string hostId, string difficulty = "Normal")
        {
            var room = await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return (false, null, "房间不存在");
            }

            if (room.HostId != hostId)
            {
                return (false, null, "仅房主可添加机器人");
            }

            if (room.CurrentPlayers >= room.MaxPlayers)
            {
                return (false, null, "房间已满");
            }

            if (room.Status != RoomStatus.Waiting && room.Status != RoomStatus.Full)
            {
                return (false, null, "房间状态不允许添加机器人");
            }

            var botName = BOT_NAMES[_botNameIndex % BOT_NAMES.Length];
            _botNameIndex++;

            var botUserId = $"bot_{Guid.NewGuid():N}";

            var botPlayer = new RoomPlayer
            {
                RoomId = roomId,
                UserId = botUserId,
                IsBot = true,
                BotName = botName,
                BotDifficulty = difficulty,
                IsReady = true,
                JoinedAt = DateTime.UtcNow
            };

            _context.RoomPlayers.Add(botPlayer);
            room.CurrentPlayers++;

            if (room.CurrentPlayers >= room.MaxPlayers)
            {
                room.Status = RoomStatus.Full;
            }

            await _context.SaveChangesAsync();

            return (true, botPlayer, $"机器人 {botName} 已加入");
        }

        public async Task<(bool Success, string Message)> RemoveBotAsync(string roomId, string hostId, string botId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return (false, "房间不存在");
            }

            if (room.HostId != hostId)
            {
                return (false, "仅房主可移除机器人");
            }

            var botPlayer = await _context.RoomPlayers
                .FirstOrDefaultAsync(p => p.Id == botId && p.RoomId == roomId && p.IsBot);

            if (botPlayer == null)
            {
                return (false, "机器人不存在");
            }

            var botName = botPlayer.BotName ?? "Bot";

            _context.RoomPlayers.Remove(botPlayer);
            room.CurrentPlayers--;

            if (room.Status == RoomStatus.Full)
            {
                room.Status = RoomStatus.Waiting;
            }

            await _context.SaveChangesAsync();

            return (true, $"机器人 {botName} 已移除");
        }
    }
}
