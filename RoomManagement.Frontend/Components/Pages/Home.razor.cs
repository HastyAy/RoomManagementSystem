using RoomManagement.Frontend.Services;
using RoomManager.Shared.DTOs;
using Radzen;
using Microsoft.JSInterop;
using RoomManager.Shared.DTOs.BookingDto;
using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class Home
    {
        // Statistics
        private int totalRooms = 0;
        private int totalCapacity = 0;
        private int activeBookings = 0;
        private int availableRooms = 0;
        private int totalStudents = 0;
        private int totalProfessors = 0;
        private int todayBookings = 0;
        private int upcomingBookings = 0;
        private int utilizationRate = 0;
        private int availabilityRate = 0;
        private int lastMonthRooms = 0;

        // UI State
        private bool loading = true;

        // Change this from a field to a property with backing field
        private string _selectedPeriod = "today";
        private string selectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (_selectedPeriod != value)
                {
                    _selectedPeriod = value;
                    OnPeriodChanged();
                }
            }
        }

        // Data collections
        private List<RoomStatusInfo> recentRooms = new();
        private List<ActivityInfo> recentActivities = new();
        private List<BookingDto> allBookings = new();
        private List<RoomDto> allRooms = new();

        // Options
        private readonly List<dynamic> periodOptions = new()
        {
            new { Text = "Today", Value = "today" },
            new { Text = "This Week", Value = "week" },
            new { Text = "This Month", Value = "month" }
        };

        // Remove the parameter since we're using property binding
        private async Task OnPeriodChanged()
        {
            // Simulate period-based data filtering
            loading = true;
            StateHasChanged();
            await Task.Delay(500); // Simulate loading
                                   // In a real app, you would filter data based on selectedPeriod
            CalculateStatistics();
            loading = false;
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            loading = true;
            try
            {
                // Load all data in parallel for better performance
                var loadTasks = new[]
                {
                    LoadRoomsData(),
                    LoadBookingsData(),
                    LoadPeopleData()
                };

                await Task.WhenAll(loadTasks);

                CalculateStatistics();
                GenerateRecentActivities();
                UpdateRoomStatuses();
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Loading Error", $"Failed to load dashboard data: {ex.Message}");
                // Set fallback data for demo
                SetFallbackData();
            }
            finally
            {
                loading = false;
                StateHasChanged();
            }
        }

        private async Task LoadRoomsData()
        {
            allRooms = await RoomService.GetAllAsync();
            totalRooms = allRooms.Count;
            totalCapacity = allRooms.Sum(r => r.Capacity);
            lastMonthRooms = Math.Max(0, totalRooms - 2); // Simulate growth
        }

        private async Task LoadBookingsData()
        {
            allBookings = await BookingService.GetAllAsync();
        }

        private async Task LoadPeopleData()
        {
            var students = await StudentService.GetAllAsync();
            var professors = await ProfessorService.GetAllAsync();
            totalStudents = students.Count;
            totalProfessors = professors.Count;
        }

        private void CalculateStatistics()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            // Active bookings (currently ongoing)
            activeBookings = allBookings.Count(b =>
                b.StartTime <= now && b.EndTime > now);

            // Available rooms
            availableRooms = totalRooms - activeBookings;

            // Today's bookings
            todayBookings = allBookings.Count(b => b.StartTime.Date == today);

            // Upcoming bookings (next 24 hours)
            upcomingBookings = allBookings.Count(b =>
                b.StartTime > now && b.StartTime <= now.AddHours(24));

            // Utilization rate
            utilizationRate = totalRooms > 0
                ? (int)Math.Round((double)activeBookings / totalRooms * 100)
                : 0;

            // Availability rate
            availabilityRate = totalRooms > 0
                ? (int)Math.Round((double)availableRooms / totalRooms * 100)
                : 100;
        }

        private void UpdateRoomStatuses()
        {
            var now = DateTime.Now;
            var roomsWithStatus = new List<RoomStatusInfo>();

            foreach (var room in allRooms.Take(8)) // Show top 8 rooms
            {
                var currentBooking = allBookings.FirstOrDefault(b =>
                    b.RoomId == room.Id && b.StartTime <= now && b.EndTime > now);

                var upcomingBooking = allBookings
                    .Where(b => b.RoomId == room.Id && b.StartTime > now)
                    .OrderBy(b => b.StartTime)
                    .FirstOrDefault();

                string status;
                string currentUser = "";

                if (currentBooking != null)
                {
                    status = "In Use";
                    currentUser = currentBooking.StudentName ?? currentBooking.ProfessorName ?? "Unknown";
                }
                else if (upcomingBooking != null && upcomingBooking.StartTime <= now.AddHours(1))
                {
                    status = "Reserved";
                    currentUser = $"Next: {upcomingBooking.StudentName ?? upcomingBooking.ProfessorName}";
                }
                else
                {
                    status = "Available";
                }

                roomsWithStatus.Add(new RoomStatusInfo
                {
                    Id = room.Id,
                    Name = room.Name,
                    Status = status,
                    CurrentUser = currentUser,
                    Capacity = room.Capacity,
                    Type = room.Type
                });
            }

            recentRooms = roomsWithStatus;
        }

        private void GenerateRecentActivities()
        {
            var activities = new List<ActivityInfo>();
            var now = DateTime.Now;

            // Recent bookings (last 24 hours)
            var recentBookings = allBookings
                .Where(b => b.CreatedAt > now.AddHours(-24))
                .OrderByDescending(b => b.CreatedAt)
                .Take(10);

            foreach (var booking in recentBookings)
            {
                var userName = booking.StudentName ?? booking.ProfessorName ?? "Unknown User";
                var timeAgo = GetTimeAgo(booking.CreatedAt);

                string status;
                string activityType;

                if (booking.StartTime <= now && booking.EndTime > now)
                {
                    status = "Active";
                    activityType = "active";
                }
                else if (booking.StartTime > now)
                {
                    status = "Scheduled";
                    activityType = "booking";
                }
                else
                {
                    status = "Completed";
                    activityType = "completed";
                }

                activities.Add(new ActivityInfo
                {
                    Type = activityType,
                    Room = booking.RoomName ?? "Unknown Room",
                    User = userName,
                    Time = timeAgo,
                    Status = status,
                    CreatedAt = booking.CreatedAt
                });
            }

            recentActivities = activities.OrderByDescending(a => a.CreatedAt).ToList();
        }

        private async Task RefreshData()
        {
            await LoadDashboardData();
            ShowSuccessNotification("Dashboard Refreshed", "All data has been updated successfully");
        }

    

        private void SetFallbackData()
        {
            totalRooms = 24;
            totalCapacity = 480;
            activeBookings = 8;
            availableRooms = 16;
            totalStudents = 150;
            totalProfessors = 25;
            todayBookings = 15;
            upcomingBookings = 12;
            utilizationRate = 33;
            availabilityRate = 67;
            lastMonthRooms = 22;

            // Sample room data
            recentRooms = new List<RoomStatusInfo>
            {
                new() { Name = "Conference Room A", Status = "In Use", CurrentUser = "John Doe" },
                new() { Name = "Study Room 1", Status = "Available", CurrentUser = "" },
                new() { Name = "Lab Room B", Status = "Reserved", CurrentUser = "Next: Jane Smith" },
                new() { Name = "Meeting Room C", Status = "Available", CurrentUser = "" }
            };

            // Sample activity data
            recentActivities = new List<ActivityInfo>
            {
                new() { Type = "booking", Room = "Conference Room A", User = "John Doe", Time = "5 minutes ago", Status = "Active" },
                new() { Type = "completed", Room = "Study Room 2", User = "Jane Smith", Time = "1 hour ago", Status = "Completed" },
                new() { Type = "booking", Room = "Lab Room C", User = "Prof. Johnson", Time = "2 hours ago", Status = "Scheduled" }
            };
        }

        // Navigation methods
        private void NavigateToBookings() => Navigation.NavigateTo("/bookings");
        private void NavigateToRooms() => Navigation.NavigateTo("/rooms");
        private void NavigateToPeople() => Navigation.NavigateTo("/people");
        private void NavigateToNewBooking() => Navigation.NavigateTo("/bookings");

        private async Task ViewRoomDetails(RoomStatusInfo room)
        {
            ShowSuccessNotification("Room Details", $"Viewing details for {room.Name}");
            // Could open a dialog or navigate to room details
        }

        // Helper methods
        private string GetWelcomeMessage()
        {
            var hour = DateTime.Now.Hour;
            var greeting = hour switch
            {
                < 12 => "Good morning",
                < 17 => "Good afternoon",
                _ => "Good evening"
            };
            return $"{greeting}! Here's your dashboard overview.";
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            return timeSpan.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)timeSpan.TotalMinutes} minutes ago",
                < 1440 => $"{(int)timeSpan.TotalHours} hours ago",
                _ => $"{(int)timeSpan.TotalDays} days ago"
            };
        }

        // Styling methods
        private string GetStatCardStyle(string color) => color switch
        {
            "blue" => "padding: 1.5rem; background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); color: white; border: none; cursor: pointer; transition: transform 0.2s ease;",
            "green" => "padding: 1.5rem; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; border: none; cursor: pointer; transition: transform 0.2s ease;",
            "purple" => "padding: 1.5rem; background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); color: white; border: none; cursor: pointer; transition: transform 0.2s ease;",
            "orange" => "padding: 1.5rem; background: linear-gradient(135deg, #f59e0b 0%, #ef4444 100%); color: white; border: none; cursor: pointer; transition: transform 0.2s ease;",
            _ => "padding: 1.5rem; background: #6b7280; color: white; border: none; cursor: pointer;"
        };

        private string GetRoomCardStyle(string status) => status switch
        {
            "In Use" => "padding: 1rem; border-left: 4px solid #ef4444; background: #fef2f2; cursor: pointer;",
            "Reserved" => "padding: 1rem; border-left: 4px solid #f59e0b; background: #fffbeb; cursor: pointer;",
            _ => "padding: 1rem; border-left: 4px solid #10b981; background: #f0fdf4; cursor: pointer;"
        };

        private BadgeStyle GetRoomStatusBadge(string status) => status switch
        {
            "In Use" => BadgeStyle.Danger,
            "Reserved" => BadgeStyle.Warning,
            _ => BadgeStyle.Success
        };

        private string GetActivityIconStyle(string type) => type switch
        {
            "active" => "display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; background: #10b981; border-radius: 50%;",
            "booking" => "display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; background: #3b82f6; border-radius: 50%;",
            _ => "display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; background: #6b7280; border-radius: 50%;"
        };

        private string GetActivityIcon(string type) => type switch
        {
            "active" => "play_circle",
            "booking" => "event",
            "completed" => "check_circle",
            _ => "schedule"
        };

        private BadgeStyle GetActivityStatusBadge(string status) => status switch
        {
            "Active" => BadgeStyle.Success,
            "Scheduled" => BadgeStyle.Info,
            "Completed" => BadgeStyle.Secondary,
            _ => BadgeStyle.Light
        };

        private string GetActionCardStyle(string color) => color switch
        {
            "blue" => "padding: 2rem; background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); color: white; border: none; width: 100%; height: 100px; cursor: pointer; transition: transform 0.2s ease;",
            "green" => "padding: 2rem; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; border: none; width: 100%; height: 100px; cursor: pointer; transition: transform 0.2s ease;",
            "purple" => "padding: 2rem; background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); color: white; border: none; width: 100%; height: 100px; cursor: pointer; transition: transform 0.2s ease;",
            _ => "padding: 2rem; background: #6b7280; color: white; border: none; width: 100%; height: 100px; cursor: pointer;"
        };

        // Notification helpers
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

        // Data models
        public class RoomStatusInfo
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public string Status { get; set; } = "";
            public string CurrentUser { get; set; } = "";
            public int Capacity { get; set; }
            public string Type { get; set; } = "";
        }

        public class ActivityInfo
        {
            public string Type { get; set; } = "";
            public string Room { get; set; } = "";
            public string User { get; set; } = "";
            public string Time { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime CreatedAt { get; set; }
        }
    }
}