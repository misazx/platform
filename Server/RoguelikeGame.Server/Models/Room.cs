using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoguelikeGame.Server.Models
{
    public enum RoomStatus
    {
        Waiting,
        Full,
        Ready,
        Playing,
        Finished
    }

    public enum GameMode
    {
        PvP,
        PvE,
        Coop
    }

    public class Room
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [Required]
        public string HostId { get; set; } = "";

        [ForeignKey("HostId")]
        public User? Host { get; set; }

        public RoomStatus Status { get; set; } = RoomStatus.Waiting;
        public GameMode Mode { get; set; } = GameMode.PvP;

        public int MaxPlayers { get; set; } = 4;
        public int CurrentPlayers { get; set; } = 1;

        public bool HasPassword { get; set; } = false;
        public string? PasswordHash { get; set; }

        public string? Seed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public ICollection<RoomPlayer> Players { get; set; } = new List<RoomPlayer>();
    }

    public class RoomPlayer
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string RoomId { get; set; } = "";

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public bool IsReady { get; set; } = false;
        public bool IsBot { get; set; } = false;
        public string? BotName { get; set; }
        public string? BotDifficulty { get; set; }
        public string? CharacterId { get; set; }
        public int Score { get; set; } = 0;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
