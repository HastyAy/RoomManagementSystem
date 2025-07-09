using Microsoft.JSInterop;
using Radzen;
using RoomManagement.Frontend.Services;
using RoomManager.Shared.Entities;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class Bookings
    {
        // Data collections
        private List<Booking> bookings = new();
        private List<Booking> filteredBookings = new();
        private List<Room> rooms = new();
        private List<Student> students = new();
        private List<Professor> professors = new();

        // Current booking being created/edited
        private Booking currentBooking = new();
        private bool isStudentBooking = true;

        // Form helpers
        private string startDateString = DateTime.Today.ToString("yyyy-MM-dd");
        private string startTimeString = "09:00";
        private string endTimeString = "10:00";

        // UI state
        private bool loading = true;
        private bool showModal = false;
        private bool isEditMode = false;
        private bool isSaving = false;

        // Filter state
        private string searchTerm = "";
        private string selectedStatusFilter = "";
        private string selectedDateFilter = "";

        // Stats
        private int totalBookings = 0;
        private int activeBookings = 0;
        private int upcomingBookings = 0;
        private int roomsInUse = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            loading = true;
            try
            {
                await LoadBookings();
                await LoadRooms();
                await LoadStudents();
                await LoadProfessors();

                FilterBookings();
                CalculateStats();
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Error loading data: {ex.Message}");
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

        private bool MatchesFilters(Booking booking)
        {
            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var search = searchTerm.ToLower();
                var roomName = GetRoomName(booking.RoomId).ToLower();
                var studentName = booking.StudentId != null ? GetStudentName(booking.StudentId.Value).ToLower() : "";
                var professorName = booking.ProfessorId != null ? GetProfessorName(booking.ProfessorId.Value).ToLower() : "";
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
        private void OpenCreateModal()
        {
            currentBooking = new Booking();
            startDateString = DateTime.Today.ToString("yyyy-MM-dd");
            startTimeString = "09:00";
            endTimeString = "10:00";
            isStudentBooking = true;
            isEditMode = false;
            showModal = true;
        }

        private void EditBooking(Booking booking)
        {
            currentBooking = new Booking
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                StudentId = booking.StudentId,
                ProfessorId = booking.ProfessorId,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Purpose = booking.Purpose
            };

            isStudentBooking = booking.StudentId != null;
            startDateString = booking.StartTime.ToString("yyyy-MM-dd");
            startTimeString = booking.StartTime.ToString("HH:mm");
            endTimeString = booking.EndTime.ToString("HH:mm");
            isEditMode = true;
            showModal = true;
        }

        private void CloseModal()
        {
            showModal = false;
            currentBooking = new Booking();
            isEditMode = false;
            isSaving = false;
            isStudentBooking = true;
        }

        private void SetStudentBooking()
        {
            isStudentBooking = true;
            currentBooking.ProfessorId = null;
            StateHasChanged();
        }

        private void SetProfessorBooking()
        {
            isStudentBooking = false;
            currentBooking.StudentId = null;
            StateHasChanged();
        }

        private bool IsFormValid()
        {
            if (currentBooking.RoomId == Guid.Empty) return false;
            if (isStudentBooking && currentBooking.StudentId == null) return false;
            if (!isStudentBooking && currentBooking.ProfessorId == null) return false;
            if (string.IsNullOrEmpty(startDateString) || string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString)) return false;

            if (!DateTime.TryParse($"{startDateString} {startTimeString}", out var start)) return false;
            if (!DateTime.TryParse($"{startDateString} {endTimeString}", out var end)) return false;
            if (end <= start) return false;

            return true;
        }

        private async Task SaveBooking()
        {
            if (!IsFormValid()) return;

            isSaving = true;
            try
            {
                var start = DateTime.Parse($"{startDateString} {startTimeString}");
                var end = DateTime.Parse($"{startDateString} {endTimeString}");

                currentBooking.StartTime = start;
                currentBooking.EndTime = end;

                if (isEditMode)
                {
                    await BookingService.UpdateAsync(currentBooking);
                    var index = bookings.FindIndex(b => b.Id == currentBooking.Id);
                    if (index >= 0)
                    {
                        bookings[index] = currentBooking;
                    }
                }
                else
                {
                    await BookingService.AddAsync(currentBooking);
                    await LoadBookings();
                }

                FilterBookings();
                CalculateStats();
                CloseModal();
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Error saving booking: {ex.Message}");
            }
            finally
            {
                isSaving = false;
            }
        }

        private async Task DeleteBooking(Booking booking)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Are you sure you want to delete this booking for {GetRoomName(booking.RoomId)}?");

            if (!confirmed) return;

            try
            {
                await BookingService.DeleteAsync(booking.Id);
                bookings.RemoveAll(b => b.Id == booking.Id);
                FilterBookings();
                CalculateStats();
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Error deleting booking: {ex.Message}");
            }
        }

        private async Task CheckIn(Booking booking)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Check-in functionality for {GetRoomName(booking.RoomId)} - Coming soon!");
        }

        // Helper methods
        private string GetRoomName(Guid roomId)
        {
            var room = rooms.FirstOrDefault(r => r.Id == roomId);
            return room?.Name ?? "Unknown Room";
        }

        private string GetStudentName(Guid studentId)
        {
            var student = students.FirstOrDefault(s => s.Id == studentId);
            return student?.LastName ?? "Unknown Student";
        }

        private string GetProfessorName(Guid professorId)
        {
            var professor = professors.FirstOrDefault(p => p.Id == professorId);
            return professor?.LastName ?? "Unknown Professor";
        }

        private string GetBookingStatus(Booking booking)
        {
            var now = DateTime.Now;
            if (booking.EndTime < now) return "completed";
            if (booking.StartTime <= now && booking.EndTime > now) return "active";
            return "upcoming";
        }

        private string GetBookingStatusClass(Booking booking)
        {
            return GetBookingStatus(booking) switch
            {
                "active" => "booking-active",
                "completed" => "booking-completed",
                _ => "booking-upcoming"
            };
        }

        private string GetStatusBadgeClass(Booking booking)
        {
            return GetBookingStatus(booking) switch
            {
                "active" => "status-active",
                "completed" => "status-completed",
                _ => "status-upcoming"
            };
        }

        private bool IsBookingActive(Booking booking)
        {
            var now = DateTime.Now;
            return booking.StartTime <= now && booking.EndTime > now;
        }

        private string GetTimeIndicator(Booking booking)
        {
            var now = DateTime.Now;
            var status = GetBookingStatus(booking);

            return status switch
            {
                "active" => $"Ends in {Math.Max(0, (int)(booking.EndTime - now).TotalHours)}h {Math.Max(0, (booking.EndTime - now).Minutes)}m",
                "upcoming" => $"Starts in {Math.Max(0, (int)(booking.StartTime - now).TotalHours)}h {Math.Max(0, (booking.StartTime - now).Minutes)}m",
                "completed" => "Completed",
                _ => ""
            };
        }
    }
}