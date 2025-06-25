using Microsoft.EntityFrameworkCore;
using RoomManager.Shared.Entities;

namespace RoomManager.BookingService.Context
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Professor> Professors => Set<Professor>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Room> Rooms => Set<Room>();
    }

}
