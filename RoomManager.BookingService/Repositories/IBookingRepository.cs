using RoomManager.BookingService.Entities;

namespace RoomManager.BookingService.Repositories
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> GetActiveBookingsAsync();
        Task<Booking?> GetByIdAsync(Guid id);
        Task<List<Booking>> GetByRoomIdAsync(Guid roomId);
        Task<List<Booking>> GetByStudentIdAsync(Guid studentId);
        Task<List<Booking>> GetByProfessorIdAsync(Guid professorId);
        Task<List<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Booking>> GetCurrentBookingsAsync();
        Task<List<Booking>> GetUpcomingBookingsAsync(int hoursAhead = 24);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Guid id);
        Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null);
        Task<List<Booking>> GetConflictingBookingsAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null);
        Task<bool> HasUserConflictAsync(Guid? studentId, Guid? professorId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null);
    }
}
