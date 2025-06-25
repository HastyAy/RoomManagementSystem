using RoomManager.Frontend.Services;
using RoomManager.Shared.Entities;

namespace RoomManagement.Frontend.Components.Components.Pages
{
    public partial class Rooms
    {
        private List<Room> rooms = new();

        protected override async Task OnInitializedAsync()
        {
            rooms = await RoomService.GetRoomsAsync();
        }

        async Task OnUpdateRoom(Room room)
        {
            await RoomService.UpdateRoomAsync(room);
        }

        void ButtonClicked()
        {
            // Handle the Click event of RadzenButton
        }
    }
}