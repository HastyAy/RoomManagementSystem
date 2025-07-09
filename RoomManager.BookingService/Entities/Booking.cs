namespace RoomManager.BookingService.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? ProfessorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Purpose { get; set; }

        // Denormalized fields for better performance and independence
        public string? RoomName { get; set; }
        public string? RoomLocation { get; set; }
        public int? RoomCapacity { get; set; }
        public string? StudentName { get; set; }
        public string? StudentMatriNumber { get; set; }
        public string? ProfessorName { get; set; }
        public string? ProfessorDepartment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Helper properties
        public string BookedByName => StudentName ?? ProfessorName ?? "Unknown";
        public TimeSpan Duration => EndTime - StartTime;
        public bool IsCurrentlyActive => DateTime.UtcNow >= StartTime && DateTime.UtcNow <= EndTime;
    }
}
