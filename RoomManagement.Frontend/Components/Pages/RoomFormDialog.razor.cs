using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManager.Shared.Entities;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class RoomFormDialog
    {
        [Parameter] public Room Room { get; set; } = new();
        [Parameter] public bool IsEdit { get; set; } = false;

        private List<string> roomTypes = new()
    {
        "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium"
    };

        private bool IsFormValid()
        {
            return !string.IsNullOrWhiteSpace(Room.Name) && Room.Capacity > 0;
        }

        private void OnSubmit()
        {
            if (IsFormValid())
            {
                DialogService.Close(Room);
            }
        }

        private void Cancel()
        {
            DialogService.Close(null);
        }
    }
}