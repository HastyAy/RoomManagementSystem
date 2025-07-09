using Microsoft.JSInterop;
using Radzen;
using RoomManagement.Frontend.Services;
using RoomManager.Shared.Entities;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class Rooms
    {
        private List<Room> rooms = new();
        private List<Room> filteredRooms = new();
        private Room currentRoom = new();

        private bool loading = true;
        private bool isEditMode = false;

        private string searchTerm = "";
        private string selectedType = "";
        private string selectedCapacity = "";

        // Stats
        private int totalRooms = 0;
        private int availableRooms = 0;
        private int totalCapacity = 0;
        private int utilizationRate = 0;

        // Dropdown options
        private List<string> roomTypes = new()
    {
        "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium"
    };

        private List<dynamic> capacityOptions = new()
    {
        new { Text = "Small (1-10)", Value = "small" },
        new { Text = "Medium (11-25)", Value = "medium" },
        new { Text = "Large (26-50)", Value = "large" },
        new { Text = "X-Large (50+)", Value = "xlarge" }
    };

        protected override async Task OnInitializedAsync()
        {
            await LoadRooms();
        }

        private async Task LoadRooms()
        {
            loading = true;
            try
            {
                rooms = await RoomService.GetAllAsync();
                FilterRooms();
                CalculateStats();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Loading Failed",
                    Detail = $"Error loading rooms: {ex.Message}",
                    Duration = 5000
                });
            }
            finally
            {
                loading = false;
            }
        }

        private void FilterRooms()
        {
            filteredRooms = rooms.Where(r => MatchesFilters(r)).ToList();
            StateHasChanged();
        }

        private bool MatchesFilters(Room room)
        {
            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var search = searchTerm.ToLower();
                if (!room.Name?.ToLower().Contains(search) == true &&
                    !room.Type?.ToLower().Contains(search) == true &&
                    !room.Location?.ToLower().Contains(search) == true)
                    return false;
            }

            // Type filter
            if (!string.IsNullOrEmpty(selectedType) && room.Type != selectedType)
                return false;

            // Capacity filter
            if (!string.IsNullOrEmpty(selectedCapacity))
            {
                var capacity = room.Capacity;
                var matches = selectedCapacity switch
                {
                    "small" => capacity <= 10,
                    "medium" => capacity > 10 && capacity <= 25,
                    "large" => capacity > 25 && capacity <= 50,
                    "xlarge" => capacity > 50,
                    _ => true
                };
                if (!matches) return false;
            }

            return true;
        }

        private void CalculateStats()
        {
            totalRooms = rooms.Count;
            availableRooms = rooms.Count; // Simplified - in real app, check booking status
            totalCapacity = rooms.Sum(r => r.Capacity);
            utilizationRate = totalRooms > 0 ? (int)Math.Round(((double)(totalRooms - availableRooms) / totalRooms) * 100) : 0;
        }

        private async Task OpenCreateModal()
        {
            currentRoom = new Room();
            isEditMode = false;
            await OpenRoomDialog();
        }

        private async Task EditRoom(Room room)
        {
            currentRoom = new Room
            {
                Id = room.Id,
                Name = room.Name,
                Type = room.Type,
                Capacity = room.Capacity,
                Location = room.Location,
                Description = room.Description
            };
            isEditMode = true;
            await OpenRoomDialog();
        }

        private async Task OpenRoomDialog()
        {
            var result = await DialogService.OpenAsync<RoomFormDialog>(
                isEditMode ? "Edit Room" : "Add New Room",
                new Dictionary<string, object>()
                    {
                { "Room", currentRoom },
                { "IsEdit", isEditMode }
                    },
                new DialogOptions()
                {
                    Width = "600px",
                    Height = "500px",
                    Resizable = true,
                    Draggable = true
                });
            var subrooms = result as Room;
            if (result != null && result is Room submittedRoom)
            {
                await SaveRoom(subrooms);
            }
        }

        private async Task SaveRoom(Room room)
        {
            try
            {
                if (isEditMode)
                {
                    await RoomService.UpdateAsync(room);
                    var index = rooms.FindIndex(r => r.Id == room.Id);
                    if (index >= 0)
                    {
                        rooms[index] = room;
                    }

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Room Updated",
                        Detail = $"Room '{room.Name}' updated successfully",
                        Duration = 3000
                    });
                }
                else
                {
                    await RoomService.AddAsync(room);
                    await LoadRooms(); // Reload to get the new room with ID

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Room Created",
                        Detail = $"Room '{room.Name}' created successfully",
                        Duration = 3000
                    });
                }

                FilterRooms();
                CalculateStats();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Save Failed",
                    Detail = $"Error saving room: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        private async Task DeleteRoom(Room room)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Are you sure you want to delete '{room.Name}'? This action cannot be undone.");

            if (!confirmed) return;

            try
            {
                await RoomService.DeleteAsync(room.Id);
                rooms.RemoveAll(r => r.Id == room.Id);
                FilterRooms();
                CalculateStats();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Room Deleted",
                    Detail = $"Room '{room.Name}' deleted successfully",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Delete Failed",
                    Detail = $"Error deleting room: {ex.Message}",
                    Duration = 5000
                });
            }
        }

  
    }
}