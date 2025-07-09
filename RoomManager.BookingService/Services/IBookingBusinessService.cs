using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.BookingDto;

namespace RoomManager.BookingService.Services
{
    public interface IBookingBusinessService
    {
        Task<ServiceResponse<BookingDto>> CreateBookingAsync(CreateBookingRequest request);
        Task<ServiceResponse<BookingDto>> UpdateBookingAsync(Guid id, UpdateBookingRequest request);
        Task<ServiceResponse<object>> DeleteBookingAsync(Guid id);
        Task<ServiceResponse<BookingDto>> GetBookingAsync(Guid id);
        Task<ServiceResponse<List<BookingDto>>> GetAllBookingsAsync();
        Task<ServiceResponse<List<BookingDto>>> GetBookingsByRoomAsync(Guid roomId);
        Task<ServiceResponse<List<BookingDto>>> GetBookingsByUserAsync(Guid? studentId, Guid? professorId);
        Task<ServiceResponse<List<BookingDto>>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResponse<bool>> CheckRoomAvailabilityAsync(Guid roomId, DateTime startTime, DateTime endTime);
    }
}
