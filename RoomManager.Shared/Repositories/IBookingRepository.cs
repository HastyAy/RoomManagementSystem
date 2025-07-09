using RoomManager.Shared.Entities;

namespace RoomManager.Shared.Repositories;

public interface IBookingRepository
{
    Task<List<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);  
    Task DeleteAsync(Guid id);
}
