using Microsoft.EntityFrameworkCore;
using RoomManager.RoomService.Domain.Entities;

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

                entity.Property(e => e.Id)
                    .HasColumnType("char(36)")
                    .IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
                // Removed the conversion since Room.Type is a string, not RoomType value object

                entity.Property(e => e.Location)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.Capacity)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime(6)")
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); // Fixed MySQL syntax

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime(6)")
                    .IsRequired(false);

                // Indexes
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasFilter("IsActive = 1");

                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Capacity);
            });

            // Configure table name and charset for MySQL
            modelBuilder.Entity<Room>().ToTable("Rooms");
        }
    }
}