using Microsoft.EntityFrameworkCore;
using RoomManager.BookingService.Entities;

 namespace RoomManager.BookingService.Context
    {
        public class BookingDbContext : DbContext
        {
            public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

            public DbSet<Booking> Bookings => Set<Booking>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Booking>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    // Required fields
                    entity.Property(e => e.RoomId).IsRequired();
                    entity.Property(e => e.StartTime).IsRequired();
                    entity.Property(e => e.EndTime).IsRequired();

                    // Optional fields with max lengths
                    entity.Property(e => e.Purpose).HasMaxLength(500);
                    entity.Property(e => e.RoomName).HasMaxLength(100);
                    entity.Property(e => e.RoomLocation).HasMaxLength(200);
                    entity.Property(e => e.StudentName).HasMaxLength(100);
                    entity.Property(e => e.StudentMatriNumber).HasMaxLength(20);
                    entity.Property(e => e.ProfessorName).HasMaxLength(100);
                    entity.Property(e => e.ProfessorDepartment).HasMaxLength(100);

                    // Indexes for performance
                    entity.HasIndex(e => e.RoomId);
                    entity.HasIndex(e => e.StudentId);
                    entity.HasIndex(e => e.ProfessorId);
                    entity.HasIndex(e => e.StartTime);
                    entity.HasIndex(e => e.EndTime);
                    entity.HasIndex(e => e.IsActive);
                    entity.HasIndex(e => new { e.RoomId, e.StartTime, e.EndTime });
                    entity.HasIndex(e => new { e.RoomId, e.IsActive });

                });
            }
        }

 }

