using Microsoft.EntityFrameworkCore;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Services
{
    public class RoomCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RoomCleanupService> _logger;
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan WaitingRoomTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan PlayingRoomTimeout = TimeSpan.FromHours(2);
        private static readonly TimeSpan EmptyRoomTimeout = TimeSpan.FromMinutes(1);

        public RoomCleanupService(
            IServiceProvider serviceProvider,
            ILogger<RoomCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[RoomCleanup] 房间清理服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredRoomsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RoomCleanup] 清理房间时发生错误");
                }

                await Task.Delay(CleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupExpiredRoomsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            var roomsToCleanup = await context.Rooms
                .Include(r => r.Players)
                .Where(r => r.Status != RoomStatus.Finished)
                .ToListAsync(ct);

            var expiredRoomIds = new List<string>();

            foreach (var room in roomsToCleanup)
            {
                bool shouldCleanup = false;
                string reason = "";

                if (room.CurrentPlayers <= 0 || room.Players.Count == 0)
                {
                    if (room.CreatedAt < now - EmptyRoomTimeout)
                    {
                        shouldCleanup = true;
                        reason = "房间为空";
                    }
                }

                if (!shouldCleanup && room.Status == RoomStatus.Waiting)
                {
                    if (room.CreatedAt < now - WaitingRoomTimeout)
                    {
                        shouldCleanup = true;
                        reason = $"等待超时({WaitingRoomTimeout.TotalMinutes}分钟)";
                    }
                }

                if (!shouldCleanup && room.Status == RoomStatus.Playing)
                {
                    if (room.StartedAt.HasValue && room.StartedAt.Value < now - PlayingRoomTimeout)
                    {
                        shouldCleanup = true;
                        reason = $"游戏超时({PlayingRoomTimeout.TotalHours}小时)";
                    }
                }

                if (shouldCleanup)
                {
                    _logger.LogInformation("[RoomCleanup] 清理房间 {RoomId} ({RoomName}): {Reason}",
                        room.Id, room.Name, reason);

                    room.Status = RoomStatus.Finished;
                    room.FinishedAt = now;
                    expiredRoomIds.Add(room.Id);
                }
            }

            if (expiredRoomIds.Count > 0)
            {
                var playersToRemove = await context.RoomPlayers
                    .Where(rp => expiredRoomIds.Contains(rp.RoomId))
                    .ToListAsync(ct);

                context.RoomPlayers.RemoveRange(playersToRemove);

                await context.SaveChangesAsync(ct);

                _logger.LogInformation("[RoomCleanup] 已清理 {Count} 个过期房间", expiredRoomIds.Count);
            }
        }
    }
}
