using Microsoft.EntityFrameworkCore;
using RoomManager.RoomService.Domain.Entities;
using RoomManager.RoomService.Domain.ValueObjects;

namespace RoomManager.RoomService.Infrastructure.Persistence
{
    public class RoomDbContext : DbContext
    {
        public RoomDbContext(DbContextOptions<RoomDbContext> options) : base(options) { }

        public DbSet<Room> Rooms => Set<Room>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasConversion(
                        v => v,  // To database
                        v => RoomType.Create(v) // From database
                    );

                entity.Property(e => e.Location)
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Capacity)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("UTC_TIMESTAMP()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false);

                // Indexes
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasFilter("IsActive = 1");

                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Capacity);
            });
        }
    }
}