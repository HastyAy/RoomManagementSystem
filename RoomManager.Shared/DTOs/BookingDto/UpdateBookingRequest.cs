using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomManager.Shared.DTOs.BookingDto
{
    public class UpdateBookingRequest
    {
        public Guid RoomId { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? ProfessorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Purpose { get; set; }
    }
}
