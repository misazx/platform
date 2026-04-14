using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoguelikeGame.Server.Models
{
	public class AchievementEntry
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
		public string AchievementId { get; set; } = "";

		public string AchievementName { get; set; } = "";

		public string Description { get; set; } = "";

		public bool IsUnlocked { get; set; } = false;

		public int Progress { get; set; } = 0;

		public int Target { get; set; } = 1;

		public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey("UserId")]
		public User? User { get; set; }
	}

	public class SaveEntry
	{
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[Required]
		public string UserId { get; set; } = "";

		[Required]
		public string PackageId { get; set; } = "";

		public int SlotId { get; set; } = 1;

		[Required]
		public string SaveData { get; set; } = "";

		public string CharacterId { get; set; } = "";

		public int CurrentFloor { get; set; } = 0;

		public int Gold { get; set; } = 0;

		public int CurrentHP { get; set; } = 0;

		public int MaxHP { get; set; } = 0;

		public bool IsVictory { get; set; } = false;

		public DateTime SavedAt { get; set; } = DateTime.UtcNow;

		public long DataSizeBytes { get; set; } = 0;

		[ForeignKey("UserId")]
		public User? User { get; set; }
	}
}
