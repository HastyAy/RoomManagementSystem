using RoomManagement.Frontend.Services;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class Home
    {
        private int totalRooms = 0;
        private int activeBookings = 0;
        private int availableRooms = 0;
        private int todayBookings = 0;
        private int utilizationRate = 0;
        private bool loading = true;

        // Sample data for demo
        private List<RoomStatus> sampleRooms = new();

        protected override async Task OnInitializedAsync()
        {
            // loading delay for better UX
            await Task.Delay(1500);

            try
            {
                // Load dashboard data
                var rooms = await RoomService.GetAllAsync();
                var bookings = await BookingService.GetAllAsync();

                totalRooms = rooms.Count;
                activeBookings = bookings.Count(b => b.StartTime <= DateTime.Now && b.EndTime > DateTime.Now);
                availableRooms = totalRooms - activeBookings;
                todayBookings = bookings.Count(b => b.StartTime.Date == DateTime.Today);
                utilizationRate = totalRooms > 0 ? (int)Math.Round((double)activeBookings / totalRooms * 100) : 0;

                // Update rooms with real data if available
                if (rooms.Any())
                {
                    sampleRooms = rooms.Take(4).Select((room, index) => new RoomStatus
                    {
                        Name = room.Name ?? $"Room {index + 1}",
                        Status = index % 3 == 0 ? "Available" : index % 3 == 1 ? "In Use" : "Maintenance"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Fallback data for demo purposes
                totalRooms = 24;
                activeBookings = 8;
                availableRooms = 16;
                todayBookings = 15;
                utilizationRate = 67;
            }

            loading = false;
            StateHasChanged();
        }



        private string GetStatusClass(string status) => status switch
        {
            "Available" => "status-available",
            "In Use" => "status-occupied",
            "Maintenance" => "status-maintenance",
            _ => "status-available"
        };

        private string GetActivityStatusClass(string status) => status switch
        {
            "confirmed" => "status-confirmed",
            "active" => "status-active",
            "pending" => "status-pending",
            "completed" => "status-completed",
            _ => "status-completed"
        };

        private string GetActivityIcon(string type) => type switch
        {
            "booking" => "<svg width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#3b82f6\" stroke-width=\"2\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"18\" rx=\"2\" ry=\"2\"></rect><line x1=\"16\" y1=\"2\" x2=\"16\" y2=\"6\"></line><line x1=\"8\" y1=\"2\" x2=\"8\" y2=\"6\"></line><line x1=\"3\" y1=\"10\" x2=\"21\" y2=\"10\"></line></svg>",
            "checkin" => "<svg width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#3b82f6\" stroke-width=\"2\"><path d=\"M22 11.08V12a10 10 0 1 1-5.93-9.14\"></path><polyline points=\"22 4 12 14.01 9 11.01\"></polyline></svg>",
            "checkout" => "<svg width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#3b82f6\" stroke-width=\"2\"><circle cx=\"12\" cy=\"12\" r=\"10\"></circle><polyline points=\"12 6 12 12 16 14\"></polyline></svg>",
            _ => "<svg width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#3b82f6\" stroke-width=\"2\"><polyline points=\"22 12 18 12 15 21 9 3 6 12 2 12\"></polyline></svg>"
        };

        private void NavigateToBookings()
        {
            Navigation.NavigateTo("/bookings");
        }

        private void NavigateToRooms()
        {
            Navigation.NavigateTo("/rooms");
        }

        private void NavigateToPeople()
        {
            Navigation.NavigateTo("/people");
        }

        // Helper classes
        public class RoomStatus
        {
            public string Name { get; set; } = "";
            public string Status { get; set; } = "";
        }

        public class ActivityItem
        {
            public string Type { get; set; } = "";
            public string Room { get; set; } = "";
            public string User { get; set; } = "";
            public string Time { get; set; } = "";
            public string Status { get; set; } = "";
        }
    }
}