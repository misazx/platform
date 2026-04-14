using Microsoft.EntityFrameworkCore;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomPlayer> RoomPlayers { get; set; }
        public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }
        public DbSet<AchievementEntry> AchievementEntries { get; set; }
        public DbSet<SaveEntry> SaveEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasOne(r => r.Host)
                      .WithMany()
                      .HasForeignKey(r => r.HostId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(r => r.Players)
                      .WithOne(rp => rp.Room)
                      .HasForeignKey(rp => rp.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RoomPlayer>(entity =>
            {
                entity.HasIndex(rp => new { rp.RoomId, rp.UserId }).IsUnique();
            });

            modelBuilder.Entity<LeaderboardEntry>(entity =>
            {
                entity.HasIndex(e => e.PackageId);
                entity.HasIndex(e => new { e.PackageId, e.Score });
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AchievementEntry>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.PackageId, e.AchievementId }).IsUnique();
                entity.HasIndex(e => e.PackageId);
            });

            modelBuilder.Entity<SaveEntry>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.PackageId, e.SlotId }).IsUnique();
                entity.HasIndex(e => e.PackageId);
            });
        }
    }
}
