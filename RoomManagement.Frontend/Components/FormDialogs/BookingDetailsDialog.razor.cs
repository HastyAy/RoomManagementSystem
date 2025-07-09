using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManager.Shared.DTOs.BookingDto;

namespace RoomManagement.Frontend.Components.FormDialogs
{
    public partial class BookingDetailsDialog
    {
        [Parameter] public BookingDto Booking { get; set; } = new();

        private void Close()
        {
            DialogService.Close();
        }

        private void CheckIn()
        {
            // In a real app, this would call an API to check in
            DialogService.Close("checkin");
        }

        private string GetDateTimeText()
        {
            return $"{Booking.StartTime:dddd, MMMM dd, yyyy} • {Booking.StartTime:HH:mm} - {Booking.EndTime:HH:mm}";
        }

        private string GetDurationText()
        {
            var duration = Booking.EndTime - Booking.StartTime;
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays} day{((int)duration.TotalDays != 1 ? "s" : "")} {duration.Hours}h";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{duration.Minutes}m";
        }

        private string GetStatusText()
        {
            var now = DateTime.Now;
            if (Booking.EndTime < now) return "Completed";
            if (Booking.StartTime <= now && Booking.EndTime > now) return "Active";
            return "Upcoming";
        }

        private BadgeStyle GetStatusBadgeStyle()
        {
            var now = DateTime.Now;
            if (Booking.EndTime < now) return BadgeStyle.Secondary;
            if (Booking.StartTime <= now && Booking.EndTime > now) return BadgeStyle.Success;
            return BadgeStyle.Info;
        }

        private bool IsBookingActive()
        {
            var now = DateTime.Now;
            return Booking.StartTime <= now && Booking.EndTime > now;
        }

        private string GetTimeIndicator()
        {
            var now = DateTime.Now;
            if (Booking.EndTime < now) return "Completed";
            if (Booking.StartTime <= now && Booking.EndTime > now)
            {
                var remaining = Booking.EndTime - now;
                return $"Ends in {GetDurationText(remaining)}";
            }
            else
            {
                var until = Booking.StartTime - now;
                return $"Starts in {GetDurationText(until)}";
            }
        }

        private string GetDurationText(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays}d {duration.Hours}h";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{Math.Max(0, duration.Minutes)}m";
        }
    }
}