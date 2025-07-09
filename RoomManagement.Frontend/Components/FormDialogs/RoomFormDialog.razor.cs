using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManagement.Frontend.Components.FormDialogs
{
    public partial class RoomFormDialog
    {
        [Parameter] public object Room { get; set; } = new CreateRoomRequest();
        [Parameter] public bool IsEdit { get; set; } = false;

        private string roomName = "";
        private string roomType = "";
        private int roomCapacity = 1;
        private string roomLocation = "";
        private string roomDescription = "";
        private bool saving = false;

        private readonly List<string> roomTypes = new()
    {
        "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium", "Office"
    };

        protected override void OnInitialized()
        {
            if (IsEdit && Room is UpdateRoomRequest updateRoom)
            {
                roomName = updateRoom.Name;
                roomType = updateRoom.Type;
                roomCapacity = updateRoom.Capacity;
                roomLocation = updateRoom.Location ?? "";
                roomDescription = updateRoom.Description ?? "";
            }
            else if (!IsEdit && Room is CreateRoomRequest createRoom)
            {
                roomName = createRoom.Name;
                roomType = createRoom.Type;
                roomCapacity = createRoom.Capacity;
                roomLocation = createRoom.Location ?? "";
                roomDescription = createRoom.Description ?? "";
            }
        }

        private async Task OnSubmit()
        {
            saving = true;

            try
            {
                if (IsEdit)
                {
                    var updateRequest = new UpdateRoomRequest
                    {
                        Name = roomName,
                        Type = roomType,
                        Capacity = roomCapacity,
                        Location = string.IsNullOrWhiteSpace(roomLocation) ? null : roomLocation,
                        Description = string.IsNullOrWhiteSpace(roomDescription) ? null : roomDescription
                    };
                    DialogService.Close(updateRequest);
                }
                else
                {
                    var createRequest = new CreateRoomRequest
                    {
                        Name = roomName,
                        Type = roomType,
                        Capacity = roomCapacity,
                        Location = string.IsNullOrWhiteSpace(roomLocation) ? null : roomLocation,
                        Description = string.IsNullOrWhiteSpace(roomDescription) ? null : roomDescription
                    };
                    DialogService.Close(createRequest);
                }
            }
            finally
            {
                saving = false;
            }
        }

        private void Cancel()
        {
            DialogService.Close();
        }
    }
}