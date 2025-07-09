using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManager.BookingService.Services
{
    public interface IRoomServiceClient
    {
        Task<RoomDto?> GetRoomAsync(Guid roomId);
        Task<bool> RoomExistsAsync(Guid roomId);
        Task<List<RoomDto>> GetAvailableRoomsAsync(int minCapacity, string? type = null);
    }
}
