namespace RoomManager.RoomService.Domain.ValueObjects
{
    public record RoomType
    {
        public static readonly RoomType Conference = new("Conference");
        public static readonly RoomType Classroom = new("Classroom");
        public static readonly RoomType Lab = new("Lab");
        public static readonly RoomType Study = new("Study");
        public static readonly RoomType Meeting = new("Meeting");
        public static readonly RoomType Auditorium = new("Auditorium");
        public static readonly RoomType Office = new("Office");

        private static readonly HashSet<string> ValidTypes = new()
        {
            "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium", "Office"
        };

        public string Value { get; }

        private RoomType(string value)
        {
            Value = value;
        }

        public static RoomType Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Room type cannot be empty", nameof(value));

            if (!ValidTypes.Contains(value))
                throw new ArgumentException($"Invalid room type: {value}", nameof(value));

            return new RoomType(value);
        }

        public static implicit operator string(RoomType roomType) => roomType.Value;
        public static explicit operator RoomType(string value) => Create(value);
    }
}