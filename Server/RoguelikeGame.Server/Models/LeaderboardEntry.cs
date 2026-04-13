using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoguelikeGame.Server.Models
{
	public class LeaderboardEntry
	{
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[Required]
		public string UserId { get; set; } = "";

		[Required]
		public string Username { get; set; } = "";

		[Required]
		public string PackageId { get; set; } = "";

		[Required]
		public long Score { get; set; } = 0;

		public int FloorReached { get; set; } = 0;

		public int KillCount { get; set; } = 0;

		public TimeSpan PlayTime { get; set; } = TimeSpan.Zero;

		public string CharacterUsed { get; set; } = "";

		public bool IsVictory { get; set; } = false;

		public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey("UserId")]
		public User? User { get; set; }
	}
}
