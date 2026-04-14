using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AchievementController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<AchievementController> _logger;

		public AchievementController(ApplicationDbContext context, ILogger<AchievementController> logger)
		{
			_context = context;
			_logger = logger;
		}

		[HttpGet("{packageId}")]
		public async Task<IActionResult> GetAchievements(string packageId, [FromQuery] string? userId = null)
		{
			try
			{
				IQueryable<AchievementEntry> query = _context.AchievementEntries.Where(a => a.PackageId == packageId);

				if (!string.IsNullOrEmpty(userId))
					query = query.Where(a => a.UserId == userId);

				var achievements = await query.OrderByDescending(a => a.UnlockedAt).Take(100).ToListAsync();

				return Ok(new { success = true, data = achievements.Select(a => new
				{
					a.Id, a.UserId, a.Username, a.PackageId,
					a.AchievementId, a.AchievementName, a.Description,
					a.IsUnlocked, a.Progress, a.Target, a.UnlockedAt
				})});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取成就失败: {PackageId}", packageId);
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[Authorize]
		[HttpPost("{packageId}/sync")]
		public async Task<IActionResult> SyncAchievements(string packageId, [FromBody] SyncAchievementsRequest request)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

			try
			{
				int synced = 0;
				foreach (var ach in request.Achievements)
				{
					var existing = await _context.AchievementEntries
						.FirstOrDefaultAsync(a => a.UserId == userId && a.PackageId == packageId && a.AchievementId == ach.AchievementId);

					if (existing == null)
					{
						_context.AchievementEntries.Add(new AchievementEntry
						{
							UserId = userId,
							Username = User.Identity?.Name ?? "Unknown",
							PackageId = packageId,
							AchievementId = ach.AchievementId,
							AchievementName = ach.AchievementName ?? ach.AchievementId,
							Description = ach.Description ?? "",
							IsUnlocked = ach.IsUnlocked,
							Progress = ach.Progress,
							Target = ach.Target,
							UnlockedAt = ach.IsUnlocked ? DateTime.UtcNow : DateTime.MinValue
						});
						synced++;
					}
					else if (ach.IsUnlocked && !existing.IsUnlocked)
					{
						existing.IsUnlocked = true;
						existing.Progress = ach.Progress;
						existing.UnlockedAt = DateTime.UtcNow;
						synced++;
					}
					else if (ach.Progress > existing.Progress)
					{
						existing.Progress = ach.Progress;
						synced++;
					}
				}

				await _context.SaveChangesAsync();
				return Ok(new { success = true, synced, message = $"同步了 {synced} 个成就" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "同步成就失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}
	}

	[ApiController]
	[Route("api/[controller]")]
	public class SaveController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<SaveController> _logger;

		public SaveController(ApplicationDbContext context, ILogger<SaveController> logger)
		{
			_context = context;
			_logger = logger;
		}

		[Authorize]
		[HttpGet("{packageId}")]
		public async Task<IActionResult> GetSaves(string packageId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

			try
			{
				var saves = await _context.SaveEntries
					.Where(s => s.UserId == userId && s.PackageId == packageId)
					.OrderBy(s => s.SlotId)
					.ToListAsync();

				return Ok(new { success = true, data = saves.Select(s => new
				{
					s.Id, s.PackageId, s.SlotId,
					s.CharacterId, s.CurrentFloor, s.Gold,
					s.CurrentHP, s.MaxHP, s.IsVictory,
					s.SavedAt, s.DataSizeBytes
				})});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取存档失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[Authorize]
		[HttpPost("{packageId}/upload")]
		public async Task<IActionResult> UploadSave(string packageId, [FromBody] UploadSaveRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

			try
			{
				var existing = await _context.SaveEntries
					.FirstOrDefaultAsync(s => s.UserId == userId && s.PackageId == packageId && s.SlotId == request.SlotId);

				if (existing != null)
				{
					existing.SaveData = request.SaveData;
					existing.CharacterId = request.CharacterId ?? "";
					existing.CurrentFloor = request.CurrentFloor;
					existing.Gold = request.Gold;
					existing.CurrentHP = request.CurrentHP;
					existing.MaxHP = request.MaxHP;
					existing.IsVictory = request.IsVictory;
					existing.SavedAt = DateTime.UtcNow;
					existing.DataSizeBytes = request.SaveData?.Length ?? 0;
				}
				else
				{
					_context.SaveEntries.Add(new SaveEntry
					{
						UserId = userId,
						PackageId = packageId,
						SlotId = request.SlotId,
						SaveData = request.SaveData ?? "",
						CharacterId = request.CharacterId ?? "",
						CurrentFloor = request.CurrentFloor,
						Gold = request.Gold,
						CurrentHP = request.CurrentHP,
						MaxHP = request.MaxHP,
						IsVictory = request.IsVictory,
						SavedAt = DateTime.UtcNow,
						DataSizeBytes = request.SaveData?.Length ?? 0
					});
				}

				await _context.SaveChangesAsync();
				_logger.LogInformation("存档上传: 用户={UserId}, 包={PackageId}, 槽位={SlotId}", userId, packageId, request.SlotId);

				return Ok(new { success = true, message = "存档已上传到服务器" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "上传存档失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[Authorize]
		[HttpGet("{packageId}/download/{slotId}")]
		public async Task<IActionResult> DownloadSave(string packageId, int slotId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

			try
			{
				var save = await _context.SaveEntries
					.FirstOrDefaultAsync(s => s.UserId == userId && s.PackageId == packageId && s.SlotId == slotId);

				if (save == null) return NotFound(new { success = false, message = "存档不存在" });

				return Ok(new
				{
					success = true,
					data = new
					{
						save.Id, save.PackageId, save.SlotId,
						save.SaveData, save.CharacterId,
						save.CurrentFloor, save.Gold,
						save.CurrentHP, save.MaxHP,
						save.IsVictory, save.SavedAt
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "下载存档失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}

		[Authorize]
		[HttpDelete("{packageId}/delete/{slotId}")]
		public async Task<IActionResult> DeleteSave(string packageId, int slotId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

			try
			{
				var save = await _context.SaveEntries
					.FirstOrDefaultAsync(s => s.UserId == userId && s.PackageId == packageId && s.SlotId == slotId);

				if (save == null) return NotFound(new { success = false, message = "存档不存在" });

				_context.SaveEntries.Remove(save);
				await _context.SaveChangesAsync();

				return Ok(new { success = true, message = "服务器存档已删除" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "删除存档失败");
				return StatusCode(500, new { success = false, message = "服务器内部错误" });
			}
		}
	}

	public class SyncAchievementsRequest
	{
		[Required]
		public List<AchievementItem> Achievements { get; set; } = new();
	}

	public class AchievementItem
	{
		[Required] public string AchievementId { get; set; } = "";
		public string? AchievementName { get; set; }
		public string? Description { get; set; }
		public bool IsUnlocked { get; set; }
		public int Progress { get; set; }
		public int Target { get; set; } = 1;
	}

	public class UploadSaveRequest
	{
		[Range(1, 3)] public int SlotId { get; set; } = 1;
		[Required] public string SaveData { get; set; } = "";
		public string? CharacterId { get; set; }
		public int CurrentFloor { get; set; }
		public int Gold { get; set; }
		public int CurrentHP { get; set; }
		public int MaxHP { get; set; }
		public bool IsVictory { get; set; }
	}
}
