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
        }
    }
}
