using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Services
{
    public interface IMatchmakingService
    {
        Task<Room?> FindMatchAsync(string userId, GameMode mode);
        Task AddToQueueAsync(string userId, GameMode mode);
        Task RemoveFromQueueAsync(string userId);
        Task<int> GetQueueCountAsync(GameMode mode);
    }

    public class MatchmakingService : IMatchmakingService
    {
        private readonly ApplicationDbContext _context;
        private static readonly Dictionary<string, (string UserId, GameMode Mode, DateTime QueuedAt)> _matchmakingQueue = new();

        public MatchmakingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Room?> FindMatchAsync(string userId, GameMode mode)
        {
            var availableRooms = await _context.Rooms
                .Include(r => r.Players)
                .Where(r =>
                    r.Mode == mode &&
                    r.Status == RoomStatus.Waiting &&
                    r.CurrentPlayers < r.MaxPlayers &&
                    !r.HasPassword)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            return availableRooms;
        }

        public async Task AddToQueueAsync(string userId, GameMode mode)
        {
            if (_matchmakingQueue.ContainsKey(userId))
            {
                _matchmakingQueue.Remove(userId);
            }

            _matchmakingQueue[userId] = (userId, mode, DateTime.UtcNow);

			var room = await FindMatchAsync(userId, mode);

			if (room != null)
			{
				await _context.Rooms
					.Where(r => r.Id == room.Id)
					.ExecuteUpdateAsync(setters => setters.SetProperty(r => r.Status, RoomStatus.Full));
			}
        }

        public Task RemoveFromQueueAsync(string userId)
        {
            if (_matchmakingQueue.ContainsKey(userId))
            {
                _matchmakingQueue.Remove(userId);
            }
            return Task.CompletedTask;
        }

        public Task<int> GetQueueCountAsync(GameMode mode)
        {
            int count = _matchmakingQueue.Count(q => q.Value.Mode == mode);
            return Task.FromResult(count);
        }
    }
}
