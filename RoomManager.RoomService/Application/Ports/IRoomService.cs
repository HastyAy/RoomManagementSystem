using RoomManager.RoomService.Application.Commands;
using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManager.RoomService.Application.Ports
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto> GetRoomByIdAsync(Guid id);
        Task<List<RoomDto>> GetRoomsByTypeAsync(string type);
        Task<List<RoomDto>> GetRoomsByCapacityAsync(int minCapacity);
        Task<RoomDto> CreateRoomAsync(CreateRoomCommand command);
        Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomCommand command);
        Task DeleteRoomAsync(Guid id);
    }
}