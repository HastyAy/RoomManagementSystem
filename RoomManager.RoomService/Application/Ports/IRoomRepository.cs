using RoomManager.RoomService.Domain.Entities;

namespace RoomManager.RoomService.Application.Ports
{
    public interface IRoomRepository
    {
        Task<List<Room>> GetAllAsync();
        Task<List<Room>> GetActiveRoomsAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task<List<Room>> GetByTypeAsync(string type);
        Task<List<Room>> GetByCapacityAsync(int minCapacity);
        Task<Room> AddAsync(Room room);
        Task<Room> UpdateAsync(Room room);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    }
}