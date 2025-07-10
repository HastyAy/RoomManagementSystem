namespace RoomManager.RoomService.Domain.Entities
{
    public class Room
    {
        private Room() { } // EF Core constructor

        public Room(string name, int capacity, string type, string? location = null, string? description = null)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Capacity = capacity > 0 ? capacity : throw new ArgumentException("Capacity must be positive", nameof(capacity));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Location = location;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public int Capacity { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public string? Location { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Domain Methods
        public void UpdateDetails(string name, int capacity, string type, string? location, string? description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Capacity = capacity > 0 ? capacity : throw new ArgumentException("Capacity must be positive", nameof(capacity));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Location = location;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool CanAccommodate(int requiredCapacity)
        {
            return IsActive && Capacity >= requiredCapacity;
        }
    }
}