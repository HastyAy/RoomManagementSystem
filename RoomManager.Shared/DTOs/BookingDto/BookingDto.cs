using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomManager.Shared.DTOs.BookingDto
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomLocation { get; set; }
        public Guid? StudentId { get; set; }
        public string? StudentName { get; set; }
        public Guid? ProfessorId { get; set; }
        public string? ProfessorName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Purpose { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
