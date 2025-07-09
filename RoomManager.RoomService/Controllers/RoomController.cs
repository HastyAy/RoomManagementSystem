using Microsoft.AspNetCore.Mvc;
using RoomManager.RoomService.Entities;
using RoomManager.RoomService.Repositories;
using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.RoomDto;


namespace RoomManager.RoomService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _repo;

        public RoomController(IRoomRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<RoomDto>>>> Get()
        {
            var rooms = await _repo.GetActiveRoomsAsync();
            var roomDtos = rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                Capacity = r.Capacity,
                Type = r.Type,
                Location = r.Location,
                Description = r.Description
            }).ToList();

            return Ok(ServiceResponse<List<RoomDto>>.SuccessResponse(roomDtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> GetById(Guid id)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null || !room.IsActive)
            {
                return NotFound(ServiceResponse<RoomDto>.ErrorResponse("Room not found"));
            }

            var roomDto = new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Type = room.Type,
                Location = room.Location,
                Description = room.Description
            };

            return Ok(ServiceResponse<RoomDto>.SuccessResponse(roomDto));
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<ServiceResponse<List<RoomDto>>>> GetByType(string type)
        {
            var rooms = await _repo.GetByTypeAsync(type);
            var roomDtos = rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                Capacity = r.Capacity,
                Type = r.Type,
                Location = r.Location,
                Description = r.Description
            }).ToList();

            return Ok(ServiceResponse<List<RoomDto>>.SuccessResponse(roomDtos));
        }

        [HttpGet("capacity/{minCapacity}")]
        public async Task<ActionResult<ServiceResponse<List<RoomDto>>>> GetByCapacity(int minCapacity)
        {
            var rooms = await _repo.GetByCapacityAsync(minCapacity);
            var roomDtos = rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                Capacity = r.Capacity,
                Type = r.Type,
                Location = r.Location,
                Description = r.Description
            }).ToList();

            return Ok(ServiceResponse<List<RoomDto>>.SuccessResponse(roomDtos));
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> Post([FromBody] CreateRoomRequest request)
        {
            if (!await _repo.IsNameUniqueAsync(request.Name))
            {
                return BadRequest(ServiceResponse<RoomDto>.ErrorResponse("Room name already exists"));
            }

            var room = new Room
            {
                Name = request.Name,
                Capacity = request.Capacity,
                Type = request.Type,
                Location = request.Location,
                Description = request.Description
            };

            await _repo.AddAsync(room);

            var roomDto = new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Type = room.Type,
                Location = room.Location,
                Description = room.Description
            };

            return CreatedAtAction(nameof(GetById), new { id = room.Id },
                ServiceResponse<RoomDto>.SuccessResponse(roomDto));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> Put(Guid id, [FromBody] UpdateRoomRequest request)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null || !room.IsActive)
            {
                return NotFound(ServiceResponse<RoomDto>.ErrorResponse("Room not found"));
            }

            if (!await _repo.IsNameUniqueAsync(request.Name, id))
            {
                return BadRequest(ServiceResponse<RoomDto>.ErrorResponse("Room name already exists"));
            }

            room.Name = request.Name;
            room.Capacity = request.Capacity;
            room.Type = request.Type;
            room.Location = request.Location;
            room.Description = request.Description;

            await _repo.UpdateAsync(room);

            var roomDto = new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Type = room.Type,
                Location = room.Location,
                Description = room.Description
            };

            return Ok(ServiceResponse<RoomDto>.SuccessResponse(roomDto));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<object>>> Delete(Guid id)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null || !room.IsActive)
            {
                return NotFound(ServiceResponse<object>.ErrorResponse("Room not found"));
            }

            await _repo.DeleteAsync(id);
            return Ok(ServiceResponse<object>.SuccessResponse(null));
        }
    }
}
