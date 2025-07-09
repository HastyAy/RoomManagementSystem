using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManager.Shared.DTOs;
using RoomManager.Shared.DTOs.BookingDto;
using RoomManager.Shared.DTOs.RoomDto;
using RoomManager.Shared.DTOs.UserDto;

namespace RoomManagement.Frontend.Components.FormDialogs
{
    public partial class BookingFormDialog
    {
        [Inject] private DialogService DialogService { get; set; } = default!;

        [Parameter] public object Booking { get; set; } = new CreateBookingRequest();
        [Parameter] public bool IsEdit { get; set; } = false;
        [Parameter] public List<RoomDto> Rooms { get; set; } = new();
        [Parameter] public List<StudentDto> Students { get; set; } = new();
        [Parameter] public List<ProfessorDto> Professors { get; set; } = new();

        private Guid selectedRoomId = Guid.Empty;
        private Guid? selectedStudentId = null;
        private Guid? selectedProfessorId = null;
        private DateTime? bookingDate = DateTime.Today;
        private DateTime? startTime = DateTime.Today.AddHours(9);
        private DateTime? endTime = DateTime.Today.AddHours(10);
        private string purpose = "";
        private string userType = "student";
        private bool saving = false;

        private RoomDto? selectedRoom => Rooms.FirstOrDefault(r => r.Id == selectedRoomId);

        private readonly List<dynamic> userTypeOptions = new()
        {
            new { Text = "Student", Value = "student" },
            new { Text = "Professor", Value = "professor" }
        };

        protected override void OnInitialized()
        {
            if (IsEdit && Booking is UpdateBookingRequest updateBooking)
            {
                selectedRoomId = updateBooking.RoomId;
                selectedStudentId = updateBooking.StudentId;
                selectedProfessorId = updateBooking.ProfessorId;
                bookingDate = updateBooking.StartTime.Date;
                startTime = updateBooking.StartTime;
                endTime = updateBooking.EndTime;
                purpose = updateBooking.Purpose ?? "";
                userType = updateBooking.StudentId.HasValue ? "student" : "professor";
            }
            else if (!IsEdit && Booking is CreateBookingRequest createBooking)
            {
                selectedRoomId = createBooking.RoomId;
                selectedStudentId = createBooking.StudentId;
                selectedProfessorId = createBooking.ProfessorId;
                if (createBooking.StartTime != default)
                {
                    bookingDate = createBooking.StartTime.Date;
                    startTime = createBooking.StartTime;
                    endTime = createBooking.EndTime;
                }
                purpose = createBooking.Purpose ?? "";
                userType = createBooking.StudentId.HasValue ? "student" : "professor";
            }
        }

        private async Task OnSubmit()
        {
            if (!IsFormValid())
            {
                return;
            }

            saving = true;

            try
            {
                var startDateTime = GetCombinedStartDateTime()!.Value;
                var endDateTime = GetCombinedEndDateTime()!.Value;

                if (IsEdit)
                {
                    var updateRequest = new UpdateBookingRequest
                    {
                        RoomId = selectedRoomId,
                        StudentId = userType == "student" ? selectedStudentId : null,
                        ProfessorId = userType == "professor" ? selectedProfessorId : null,
                        StartTime = startDateTime,
                        EndTime = endDateTime,
                        Purpose = string.IsNullOrWhiteSpace(purpose) ? null : purpose
                    };
                    DialogService.Close(updateRequest);
                }
                else
                {
                    var createRequest = new CreateBookingRequest
                    {
                        RoomId = selectedRoomId,
                        StudentId = userType == "student" ? selectedStudentId : null,
                        ProfessorId = userType == "professor" ? selectedProfessorId : null,
                        StartTime = startDateTime,
                        EndTime = endDateTime,
                        Purpose = string.IsNullOrWhiteSpace(purpose) ? null : purpose
                    };
                    DialogService.Close(createRequest);
                }
            }
            catch (Exception ex)
            {
                // You might want to show an error message to the user here
                // For example: NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
                System.Diagnostics.Debug.WriteLine($"Error submitting booking: {ex.Message}");
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

        private void OnRoomChanged()
        {
            StateHasChanged();
        }

        private void OnUserTypeChanged()
        {
            // Clear previous selections when switching user type
            selectedStudentId = null;
            selectedProfessorId = null;
            StateHasChanged();
        }

        private void OnDateTimeChanged()
        {
            StateHasChanged();
        }

        private DateTime? GetCombinedStartDateTime()
        {
            if (!bookingDate.HasValue || !startTime.HasValue) return null;
            return bookingDate.Value.Date.Add(startTime.Value.TimeOfDay);
        }

        private DateTime? GetCombinedEndDateTime()
        {
            if (!bookingDate.HasValue || !endTime.HasValue) return null;
            return bookingDate.Value.Date.Add(endTime.Value.TimeOfDay);
        }

        private bool IsFormValid()
        {
            if (selectedRoomId == Guid.Empty) return false;
            if (userType == "student" && !selectedStudentId.HasValue) return false;
            if (userType == "professor" && !selectedProfessorId.HasValue) return false;
            if (!bookingDate.HasValue || !startTime.HasValue || !endTime.HasValue) return false;

            var start = GetCombinedStartDateTime();
            var end = GetCombinedEndDateTime();
            if (!start.HasValue || !end.HasValue || end <= start) return false;
            if (start < DateTime.Now.AddMinutes(-5)) return false; // Allow 5 minutes tolerance

            return true;
        }

        private string GetDurationText()
        {
            var start = GetCombinedStartDateTime();
            var end = GetCombinedEndDateTime();
            if (!start.HasValue || !end.HasValue) return "";

            var duration = end.Value - start.Value;
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays} day{((int)duration.TotalDays != 1 ? "s" : "")} {duration.Hours}h {duration.Minutes}m";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{duration.Minutes} minutes";
        }
    }
}