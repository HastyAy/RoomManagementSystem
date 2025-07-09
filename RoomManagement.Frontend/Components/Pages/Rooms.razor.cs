using Microsoft.JSInterop;
using Radzen;
using RoomManagement.Frontend.Components.FormDialogs;
using RoomManagement.Frontend.Services;
using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class Rooms
    {
        private List<RoomDto> rooms = new();
        private List<RoomDto> filteredRooms = new();
        private CreateRoomRequest currentRoom = new();
        private UpdateRoomRequest updateRoom = new();

        private bool loading = true;
        private bool isEditMode = false;
        private Guid editingRoomId = Guid.Empty;

        private string searchTerm = "";
        private string selectedType = "";
        private string selectedCapacity = "";

        // Stats
        private int totalRooms = 0;
        private int roomTypesCount = 0;
        private int totalCapacity = 0;
        private int averageCapacity = 0;

        // Dropdown options
        private readonly List<string> roomTypes = new()
        {
            "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium", "Office"
        };

        private readonly List<dynamic> capacityOptions = new()
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
                ShowErrorNotification("Loading Failed", $"Error loading rooms: {ex.Message}");
            }
            finally
            {
                loading = false;
            }
        }

        private void FilterRooms()
        {
            filteredRooms = rooms.Where(MatchesFilters).ToList();
            StateHasChanged();
        }

        private bool MatchesFilters(RoomDto room)
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
            roomTypesCount = rooms.Select(r => r.Type).Distinct().Count();
            totalCapacity = rooms.Sum(r => r.Capacity);
            averageCapacity = totalRooms > 0 ? (int)Math.Round((double)totalCapacity / totalRooms) : 0;
        }

        private async Task OpenCreateModal()
        {
            currentRoom = new CreateRoomRequest();
            isEditMode = false;
            editingRoomId = Guid.Empty;
            await OpenRoomDialog();
        }

        private async Task EditRoom(RoomDto room)
        {
            updateRoom = new UpdateRoomRequest
            {
                Name = room.Name,
                Type = room.Type,
                Capacity = room.Capacity,
                Location = room.Location,
                Description = room.Description
            };
            isEditMode = true;
            editingRoomId = room.Id;
            await OpenRoomDialog();
        }

        private async Task OpenRoomDialog()
        {
            var parameters = new Dictionary<string, object>();

            if (isEditMode)
            {
                parameters.Add("Room", updateRoom);
                parameters.Add("IsEdit", true);
            }
            else
            {
                parameters.Add("Room", currentRoom);
                parameters.Add("IsEdit", false);
            }

            var result = await DialogService.OpenAsync<RoomFormDialog>(
                isEditMode ? "Edit Room" : "Add New Room",
                parameters,
                new DialogOptions()
                {
                    Width = "600px",
                    Height = "500px",
                    Resizable = true,
                    Draggable = true
                });

            if (result != null)
            {
                await SaveRoom(result);
            }
        }

        private async Task SaveRoom(object roomData)
        {
            try
            {
                bool success;
                string roomName;

                if (isEditMode && roomData is UpdateRoomRequest updateRequest)
                {
                    success = await RoomService.UpdateAsync(editingRoomId, updateRequest);
                    roomName = updateRequest.Name;

                    if (success)
                    {
                        ShowSuccessNotification("Room Updated", $"Room '{roomName}' updated successfully");
                    }
                }
                else if (!isEditMode && roomData is CreateRoomRequest createRequest)
                {
                    success = await RoomService.AddAsync(createRequest);
                    roomName = createRequest.Name;

                    if (success)
                    {
                        ShowSuccessNotification("Room Created", $"Room '{roomName}' created successfully");
                    }
                }
                else
                {
                    ShowErrorNotification("Save Failed", "Invalid room data");
                    return;
                }

                if (success)
                {
                    await LoadRooms(); // Reload to get fresh data
                }
                else
                {
                    ShowErrorNotification("Save Failed", "Failed to save room");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Save Failed", $"Error saving room: {ex.Message}");
            }
        }

        private async Task DeleteRoom(RoomDto room)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Are you sure you want to delete '{room.Name}'? This action cannot be undone.");

            if (!confirmed) return;

            try
            {
                var success = await RoomService.DeleteAsync(room.Id);

                if (success)
                {
                    await LoadRooms(); // Reload to get fresh data
                    ShowSuccessNotification("Room Deleted", $"Room '{room.Name}' deleted successfully");
                }
                else
                {
                    ShowErrorNotification("Delete Failed", "Failed to delete room");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Delete Failed", $"Error deleting room: {ex.Message}");
            }
        }

        private async Task ViewRoomDetails(RoomDto room)
        {
            await DialogService.OpenAsync<RoomDetailsDialog>(
                $"Room Details - {room.Name}",
                new Dictionary<string, object> { { "Room", room } },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "400px",
                    Resizable = true,
                    Draggable = true
                });
        }

        private void ShowSuccessNotification(string summary, string detail)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = summary,
                Detail = detail,
                Duration = 3000
            });
        }

        private void ShowErrorNotification(string summary, string detail)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = summary,
                Detail = detail,
                Duration = 5000
            });
        }
    }
}