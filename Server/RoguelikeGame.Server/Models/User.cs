using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoguelikeGame.Server.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [EmailAddress]
        public string? Email { get; set; }

        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;

        public int TotalGamesPlayed { get; set; } = 0;
        public int GamesWon { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public bool IsOnline { get; set; } = false;
        public string? ConnectionId { get; set; }
    }
}
