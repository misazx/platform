using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoguelikeGame.Server.Models
{
    public class Friendship
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string RequesterId { get; set; } = "";

        [Required]
        public string AddresseeId { get; set; } = "";

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AcceptedAt { get; set; }

        [ForeignKey("RequesterId")]
        public User? Requester { get; set; }

        [ForeignKey("AddresseeId")]
        public User? Addressee { get; set; }
    }

    public enum FriendshipStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
