using Microsoft.JSInterop;
using Radzen;
using RoomManagement.Frontend.Components.FormDialogs;
using RoomManagement.Frontend.Services;
using RoomManager.Shared.DTOs;
using RoomManager.Shared.DTOs.BookingDto;
using RoomManager.Shared.DTOs.RoomDto;
using RoomManager.Shared.DTOs.UserDto;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class Bookings
    {
        // Data collections
        private List<BookingDto> bookings = new();
        private List<BookingDto> filteredBookings = new();
        private List<RoomDto> rooms = new();
        private List<StudentDto> students = new();
        private List<ProfessorDto> professors = new();

        // Current booking being created/edited
        private CreateBookingRequest currentCreateBooking = new();
        private UpdateBookingRequest currentUpdateBooking = new();
        private Guid editingBookingId = Guid.Empty;
        private bool isStudentBooking = true;

        // UI state
        private bool loading = true;
        private bool isEditMode = false;

        // Filter state
        private string searchTerm = "";
        private string selectedStatusFilter = "";
        private string selectedDateFilter = "";

        // Stats
        private int totalBookings = 0;
        private int activeBookings = 0;
        private int upcomingBookings = 0;
        private int roomsInUse = 0;

        // Dropdown options
        private readonly List<dynamic> statusOptions = new()
        {
            new { Text = "Active", Value = "active" },
            new { Text = "Upcoming", Value = "upcoming" },
            new { Text = "Completed", Value = "completed" }
        };

        private readonly List<dynamic> dateOptions = new()
        {
            new { Text = "Today", Value = "today" },
            new { Text = "This Week", Value = "week" },
            new { Text = "This Month", Value = "month" }
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            loading = true;
            try
            {
                await Task.WhenAll(
                    LoadBookings(),
                    LoadRooms(),
                    LoadStudents(),
                    LoadProfessors()
                );

                FilterBookings();
                CalculateStats();
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Loading Failed", $"Error loading data: {ex.Message}");
            }
            finally
            {
                loading = false;
            }
        }

        private async Task LoadBookings()
        {
            bookings = await BookingService.GetAllAsync();
        }

        private async Task LoadRooms()
        {
            rooms = await RoomService.GetAllAsync();
        }

        private async Task LoadStudents()
        {
            students = await StudentService.GetAllAsync();
        }

        private async Task LoadProfessors()
        {
            professors = await ProfessorService.GetAllAsync();
        }

        private void FilterBookings()
        {
            filteredBookings = bookings.Where(MatchesFilters).ToList();
            StateHasChanged();
        }

        private bool MatchesFilters(BookingDto booking)
        {
            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var search = searchTerm.ToLower();
                var roomName = booking.RoomName?.ToLower() ?? "";
                var studentName = booking.StudentName?.ToLower() ?? "";
                var professorName = booking.ProfessorName?.ToLower() ?? "";
                var purpose = booking.Purpose?.ToLower() ?? "";

                if (!roomName.Contains(search) &&
                    !studentName.Contains(search) &&
                    !professorName.Contains(search) &&
                    !purpose.Contains(search))
                    return false;
            }

            // Status filter
            if (!string.IsNullOrEmpty(selectedStatusFilter))
            {
                var status = GetBookingStatus(booking);
                if (status != selectedStatusFilter) return false;
            }

            // Date filter
            if (!string.IsNullOrEmpty(selectedDateFilter))
            {
                var now = DateTime.Now;
                var matches = selectedDateFilter switch
                {
                    "today" => booking.StartTime.Date == DateTime.Today,
                    "week" => booking.StartTime >= now.Date.AddDays(-(int)now.DayOfWeek) &&
                             booking.StartTime < now.Date.AddDays(7 - (int)now.DayOfWeek),
                    "month" => booking.StartTime.Month == now.Month && booking.StartTime.Year == now.Year,
                    _ => true
                };
                if (!matches) return false;
            }

            return true;
        }

        private void CalculateStats()
        {
            var now = DateTime.Now;
            totalBookings = bookings.Count;
            activeBookings = bookings.Count(b => b.StartTime <= now && b.EndTime > now);
            upcomingBookings = bookings.Count(b => b.StartTime > now);
            roomsInUse = bookings.Where(b => b.StartTime <= now && b.EndTime > now)
                               .Select(b => b.RoomId)
                               .Distinct()
                               .Count();
        }

        // Modal handling
        private async Task OpenCreateModal()
        {
            currentCreateBooking = new CreateBookingRequest
            {
                StartTime = DateTime.Today.AddHours(9),
                EndTime = DateTime.Today.AddHours(10)
            };
            isStudentBooking = true;
            isEditMode = false;
            editingBookingId = Guid.Empty;
            await OpenBookingDialog();
        }

        private async Task EditBooking(BookingDto booking)
        {
            currentUpdateBooking = new UpdateBookingRequest
            {
                RoomId = booking.RoomId,
                StudentId = booking.StudentId,
                ProfessorId = booking.ProfessorId,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Purpose = booking.Purpose
            };

            isStudentBooking = booking.StudentId != null;
            isEditMode = true;
            editingBookingId = booking.Id;
            await OpenBookingDialog();
        }

        private async Task OpenBookingDialog()
        {
            var parameters = new Dictionary<string, object>
            {
                { "Rooms", rooms },
                { "Students", students },
                { "Professors", professors },
                { "IsEdit", isEditMode }
            };

            if (isEditMode)
            {
                parameters.Add("Booking", currentUpdateBooking);
            }
            else
            {
                parameters.Add("Booking", currentCreateBooking);
            }

            var result = await DialogService.OpenAsync<BookingFormDialog>(
                isEditMode ? "Edit Booking" : "Create New Booking",
                parameters,
                new DialogOptions()
                {
                    Width = "700px",
                    Height = "600px",
                    Resizable = true,
                    Draggable = true
                });

            if (result != null)
            {
                await SaveBooking(result);
            }
        }

        private async Task SaveBooking(object bookingData)
        {
            try
            {
                bool success;
                string operation;

                if (isEditMode && bookingData is UpdateBookingRequest updateRequest)
                {
                    success = await BookingService.UpdateAsync(editingBookingId, updateRequest);
                    operation = "updated";
                }
                else if (!isEditMode && bookingData is CreateBookingRequest createRequest)
                {
                    success = await BookingService.AddAsync(createRequest);
                    operation = "created";
                }
                else
                {
                    ShowErrorNotification("Save Failed", "Invalid booking data");
                    return;
                }

                if (success)
                {
                    await LoadBookings(); // Reload to get fresh data
                    FilterBookings();
                    CalculateStats();
                    ShowSuccessNotification("Booking Saved", $"Booking {operation} successfully");
                }
                else
                {
                    ShowErrorNotification("Save Failed", $"Failed to {operation.TrimEnd('d')} booking");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Save Failed", $"Error saving booking: {ex.Message}");
            }
        }

        private async Task DeleteBooking(BookingDto booking)
        {
            var roomName = booking.RoomName ?? "Unknown Room";
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Are you sure you want to delete the booking for {roomName}? This action cannot be undone.");

            if (!confirmed) return;

            try
            {
                var success = await BookingService.DeleteAsync(booking.Id);

                if (success)
                {
                    await LoadBookings(); // Reload to get fresh data
                    FilterBookings();
                    CalculateStats();
                    ShowSuccessNotification("Booking Deleted", $"Booking for {roomName} deleted successfully");
                }
                else
                {
                    ShowErrorNotification("Delete Failed", "Failed to delete booking");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Delete Failed", $"Error deleting booking: {ex.Message}");
            }
        }

        private async Task ViewBookingDetails(BookingDto booking)
        {
            await DialogService.OpenAsync<BookingDetailsDialog>(
                $"Booking Details - {booking.RoomName}",
                new Dictionary<string, object> { { "Booking", booking } },
                new DialogOptions()
                {
                    Width = "600px",
                    Height = "500px",
                    Resizable = true,
                    Draggable = true
                });
        }

        private async Task CheckIn(BookingDto booking)
        {
            // In a real app, this would call an API to check in
            ShowSuccessNotification("Check In", $"Checked in to {booking.RoomName ?? "room"}");
            await Task.CompletedTask; // Placeholder for actual check-in logic
        }

        // Helper methods
        private string GetBookingStatus(BookingDto booking)
        {
            var now = DateTime.Now;
            if (booking.EndTime < now) return "completed";
            if (booking.StartTime <= now && booking.EndTime > now) return "active";
            return "upcoming";
        }

        private string GetBookingStatusText(BookingDto booking)
        {
            return GetBookingStatus(booking) switch
            {
                "active" => "Active",
                "completed" => "Completed",
                _ => "Upcoming"
            };
        }

        private BadgeStyle GetStatusBadgeStyle(BookingDto booking)
        {
            return GetBookingStatus(booking) switch
            {
                "active" => BadgeStyle.Success,
                "completed" => BadgeStyle.Secondary,
                _ => BadgeStyle.Info
            };
        }

        private string GetStatusColor(BookingDto booking)
        {
            return GetBookingStatus(booking) switch
            {
                "active" => "#10b981",
                "completed" => "#6b7280",
                _ => "#3b82f6"
            };
        }

        private bool IsBookingActive(BookingDto booking)
        {
            var now = DateTime.Now;
            return booking.StartTime <= now && booking.EndTime > now;
        }

        private string GetTimeIndicator(BookingDto booking)
        {
            var now = DateTime.Now;
            var status = GetBookingStatus(booking);

            return status switch
            {
                "active" => $"Ends in {GetDurationText(booking.EndTime - now)}",
                "upcoming" => $"Starts in {GetDurationText(booking.StartTime - now)}",
                "completed" => "Completed",
                _ => ""
            };
        }

        private string GetDurationText(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays}d {duration.Hours}h";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{Math.Max(0, duration.Minutes)}m";
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