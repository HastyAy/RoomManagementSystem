using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomManager.Shared.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }
        public Room? Room { get; set; }

        public Guid? StudentId { get; set; }
        public Student? Student { get; set; }

        public Guid? ProfessorId { get; set; }
        public Professor? Professor { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string? Purpose { get; set; }
    }

}
