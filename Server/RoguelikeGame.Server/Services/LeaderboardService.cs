using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Services
{
	public interface ILeaderboardService
	{
		Task<LeaderboardEntry> SubmitScoreAsync(string userId, string username, string packageId, long score,
			int floorReached = 0, int killCount = 0, double playTimeSeconds = 0,
			string characterUsed = "", bool isVictory = false);

		Task<List<LeaderboardEntry>> GetTopScoresAsync(string packageId, int topN = 100);

		Task<List<LeaderboardEntry>> GetUserScoresAsync(string userId, string packageId, int limit = 10);

		Task<int> GetUserRankAsync(string userId, string packageId);

		Task<Dictionary<string, object>> GetPackageStatsAsync(string packageId);
	}

	public class LeaderboardService : ILeaderboardService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<LeaderboardService> _logger;

	public LeaderboardService(ApplicationDbContext context, ILogger<LeaderboardService> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<LeaderboardEntry> SubmitScoreAsync(string userId, string username, string packageId, long score,
		int floorReached = 0, int killCount = 0, double playTimeSeconds = 0,
		string characterUsed = "", bool isVictory = false)
	{
		try
		{
			var entry = new LeaderboardEntry
			{
				UserId = userId,
				Username = username,
				PackageId = packageId,
				Score = score,
				FloorReached = floorReached,
				KillCount = killCount,
				PlayTime = TimeSpan.FromSeconds(playTimeSeconds),
				CharacterUsed = characterUsed,
				IsVictory = isVictory,
				PlayedAt = DateTime.UtcNow
			};

			_context.LeaderboardEntries.Add(entry);
			await _context.SaveChangesAsync();

			var user = await _context.Users.FindAsync(userId);
			if (user != null)
			{
				user.TotalGamesPlayed++;
				if (isVictory) user.GamesWon++;
				user.Experience += (int)(score / 10);
				await _context.SaveChangesAsync();
			}

			_logger.LogInformation("排行榜记录: 用户={Username}, 包={PackageId}, 分数={Score}, 胜利={IsVictory}",
				username, packageId, score, isVictory);

			return entry;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "提交分数失败");
			throw;
		}
	}

	public async Task<List<LeaderboardEntry>> GetTopScoresAsync(string packageId, int topN = 100)
	{
		return await _context.LeaderboardEntries
			.Where(e => e.PackageId == packageId)
			.OrderByDescending(e => e.Score)
			.ThenBy(e => e.PlayedAt)
			.Take(topN)
			.ToListAsync();
	}

	public async Task<List<LeaderboardEntry>> GetUserScoresAsync(string userId, string packageId, int limit = 10)
	{
		return await _context.LeaderboardEntries
			.Where(e => e.UserId == userId && e.PackageId == packageId)
			.OrderByDescending(e => e.Score)
			.ThenBy(e => e.PlayedAt)
			.Take(limit)
			.ToListAsync();
	}

	public async Task<int> GetUserRankAsync(string userId, string packageId)
	{
		var userBestScore = await _context.LeaderboardEntries
			.Where(e => e.UserId == userId && e.PackageId == packageId)
			.MaxAsync(e => (long?)e.Score);

		if (!userBestScore.HasValue) return -1;

		var rank = await _context.LeaderboardEntries
			.Where(e => e.PackageId == packageId && e.Score > userBestScore.Value)
			.CountAsync();

		return rank + 1;
	}

	public async Task<Dictionary<string, object>> GetPackageStatsAsync(string packageId)
	{
		var entries = await _context.LeaderboardEntries
			.Where(e => e.PackageId == packageId)
			.ToListAsync();

		return new Dictionary<string, object>
		{
			{ "totalGames", entries.Count },
			{ "uniquePlayers", entries.Select(e => e.UserId).Distinct().Count() },
			{ "victories", entries.Count(e => e.IsVictory) },
			{ "highestScore", entries.Any() ? entries.Max(e => e.Score) : 0L },
			{ "avgScore", entries.Any() ? (long)entries.Average(e => e.Score) : 0L },
			{ "avgFloor", entries.Any() ? entries.Average(e => e.FloorReached) : 0.0 }
		};
	}
	}
}
