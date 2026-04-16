using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoguelikeGame.Server.Models;
using RoguelikeGame.Server.Services;

namespace RoguelikeGame.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LeaderboardController : ControllerBase
	{
		private readonly ILeaderboardService _leaderboardService;
		private readonly ILogger<LeaderboardController> _logger;

		public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
		{
			_leaderboardService = leaderboardService;
			_logger = logger;
		}

		[HttpGet("{packageId}/top")]
		public async Task<IActionResult> GetTopScores(string packageId, [FromQuery] int top = 100)
		{
			try
			{
				var entries = await _leaderboardService.GetTopScoresAsync(packageId, Math.Min(top, 200));
				var stats = await _leaderboardService.GetPackageStatsAsync(packageId);

				var result = entries.Select((e, index) => new
				{
					rank = index + 1,
					e.Id,
					e.UserId,
					e.Username,
					e.Score,
					e.FloorReached,
					e.KillCount,
					playTimeSeconds = (int)e.PlayTime.TotalSeconds,
					e.CharacterUsed,
					e.IsVictory,
					e.PlayedAt
				});

				return Ok(new { success = true, data = result, stats });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取排行榜失败: {PackageId}", packageId);
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[HttpGet("{packageId}/user/{userId}")]
		public async Task<IActionResult> GetUserScores(string packageId, string userId)
		{
			try
			{
				var entries = await _leaderboardService.GetUserScoresAsync(userId, packageId);
				var rank = await _leaderboardService.GetUserRankAsync(userId, packageId);

				var result = entries.Select(e => new
				{
					e.Id,
					e.Score,
					e.FloorReached,
					e.KillCount,
					playTimeSeconds = (int)e.PlayTime.TotalSeconds,
					e.CharacterUsed,
					e.IsVictory,
					e.PlayedAt
				});

				return Ok(new { success = true, data = result, bestRank = rank });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取用户分数失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[HttpGet("{packageId}/rank")]
		[Authorize]
		public async Task<IActionResult> GetUserRank(string packageId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
				return Unauthorized(new { success = false, message = "未登录" });

			try
			{
				int rank = await _leaderboardService.GetUserRankAsync(userId, packageId);
				return Ok(new { success = true, rank });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取排名失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[HttpGet("packages")]
		public async Task<IActionResult> GetAllPackagesLeaderboard()
		{
			try
			{
				var packages = new[] { "base_game", "light_shadow_traveler" };
				var results = new List<object>();

				foreach (var pkg in packages)
				{
					var top3 = await _leaderboardService.GetTopScoresAsync(pkg, 3);
					var stats = await _leaderboardService.GetPackageStatsAsync(pkg);

					results.Add(new
					{
						packageId = pkg,
						top3 = top3.Select((e, i) => new { rank = i + 1, e.Username, e.Score, e.IsVictory }),
						stats
					});
				}

				return Ok(new { success = true, data = results });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取全部排行榜失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[Authorize]
		[HttpPost("{packageId}/submit")]
		public async Task<IActionResult> SubmitScore(string packageId, [FromBody] SubmitScoreRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
				return Unauthorized(new { success = false, message = "未登录" });

			try
			{
				var entry = await _leaderboardService.SubmitScoreAsync(
					userId,
					request.Username ?? User.Identity?.Name ?? "Unknown",
					packageId,
					request.Score,
					request.FloorReached,
					request.KillCount,
					request.PlayTimeSeconds,
					request.CharacterUsed ?? "",
					request.IsVictory
				);

				int rank = await _leaderboardService.GetUserRankAsync(userId, packageId);

				return Ok(new { success = true, entryId = entry.Id, rank, message = "分数已提交！" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "提交分数失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}
	}

	public class SubmitScoreRequest
	{
		[Required]
		public long Score { get; set; }

		public int FloorReached { get; set; } = 0;

		public int KillCount { get; set; } = 0;

		public double PlayTimeSeconds { get; set; } = 0;

		public string? CharacterUsed { get; set; }

		public bool IsVictory { get; set; } = false;

		public string? Username { get; set; }
	}
}
