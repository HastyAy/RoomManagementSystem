namespace RoomManager.RoomService.Application.Commands
{
    public record UpdateRoomCommand(
        string Name,
        int Capacity,
        string Type,
        string? Location = null,
        string? Description = null
    );
}