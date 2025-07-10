// Extension Methods für bessere Usability
using RoomManagement.Frontend.Services;
using RoomManager.Shared.DTOs.RoomDto;

public static class RoomServiceExtensions
{
    public static async Task<List<RoomDto>> GetAvailableRoomsAsync(this RoomService service, int requiredCapacity)
    {
        var allRooms = await service.GetAllAsync();
        return allRooms.Where(r => r.Capacity >= requiredCapacity).ToList();
    }

    public static async Task<List<RoomDto>> SearchRoomsAsync(this RoomService service, string searchTerm)
    {
        var allRooms = await service.GetAllAsync();

        if (string.IsNullOrWhiteSpace(searchTerm))
            return allRooms;

        var term = searchTerm.ToLower();
        return allRooms.Where(r =>
            r.Name.ToLower().Contains(term) ||
            r.Type.ToLower().Contains(term) ||
            (r.Location?.ToLower().Contains(term) ?? false) ||
            (r.Description?.ToLower().Contains(term) ?? false)
        ).ToList();
    }
}