using Microsoft.EntityFrameworkCore;
using RoomManager.BookingService.Context;
using RoomManager.BookingService.Entities;

namespace RoomManager.BookingService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context) => _context = context;

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetActiveBookingsAsync()
        {
            return await _context.Bookings
                .Where(b => b.IsActive)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
        }

        public async Task<List<Booking>> GetByRoomIdAsync(Guid roomId)
        {
            return await _context.Bookings
                .Where(b => b.RoomId == roomId && b.IsActive)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByStudentIdAsync(Guid studentId)
        {
            return await _context.Bookings
                .Where(b => b.StudentId == studentId && b.IsActive)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByProfessorIdAsync(Guid professorId)
        {
            return await _context.Bookings
                .Where(b => b.ProfessorId == professorId && b.IsActive)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Bookings
                .Where(b => b.IsActive &&
                           b.StartTime >= startDate &&
                           b.EndTime <= endDate)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetCurrentBookingsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Bookings
                .Where(b => b.IsActive &&
                           b.StartTime <= now &&
                           b.EndTime >= now)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetUpcomingBookingsAsync(int hoursAhead = 24)
        {
            var now = DateTime.UtcNow;
            var futureTime = now.AddHours(hoursAhead);

            return await _context.Bookings
                .Where(b => b.IsActive &&
                           b.StartTime >= now &&
                           b.StartTime <= futureTime)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task AddAsync(Booking booking)
        {
            booking.Id = Guid.NewGuid();
            booking.CreatedAt = DateTime.UtcNow;
            booking.IsActive = true;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.IsActive = false;
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null)
        {
            var conflictingBookings = await GetConflictingBookingsAsync(roomId, startTime, endTime, excludeBookingId);
            return !conflictingBookings.Any();
        }

        public async Task<List<Booking>> GetConflictingBookingsAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null)
        {
            var query = _context.Bookings.Where(b =>
                b.RoomId == roomId &&
                b.IsActive &&
                b.StartTime < endTime &&
                b.EndTime > startTime);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> HasUserConflictAsync(Guid? studentId, Guid? professorId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null)
        {
            var query = _context.Bookings.Where(b =>
                b.IsActive &&
                b.StartTime < endTime &&
                b.EndTime > startTime &&
                ((studentId.HasValue && b.StudentId == studentId.Value) ||
                 (professorId.HasValue && b.ProfessorId == professorId.Value)));

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
