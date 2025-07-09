using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomManager.Shared.DTOs.RoomDto
{
    public class UpdateRoomRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
}
