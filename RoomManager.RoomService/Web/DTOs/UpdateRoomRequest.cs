using System.ComponentModel.DataAnnotations;

namespace RoomManager.RoomService.Web.DTOs
{
    /// <summary>
    /// Request to update an existing room
    /// </summary>
    public record UpdateRoomRequest
    {
        /// <summary>
        /// The room name (must be unique)
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The room capacity (number of people)
        /// </summary>
        [Required]
        [Range(1, 1000)]
        public int Capacity { get; init; }

        /// <summary>
        /// The room type (Conference, Classroom, Lab, Study, Meeting, Auditorium, Office)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; init; } = string.Empty;

        /// <summary>
        /// The room location (optional)
        /// </summary>
        [StringLength(200)]
        public string? Location { get; init; }

        /// <summary>
        /// Room description (optional)
        /// </summary>
        [StringLength(500)]
        public string? Description { get; init; }
    }
}