using Microsoft.EntityFrameworkCore;
using RoomManager.BookingService.Context;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;

namespace RoomManager.BookingService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context) => _context = context;

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Student)
                .Include(b => b.Professor)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Student)
                .Include(b => b.Professor)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Booking booking)
        {
            booking.Id = Guid.NewGuid();
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }
    }

}
