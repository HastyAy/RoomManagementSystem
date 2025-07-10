using Microsoft.AspNetCore.Mvc;
using RoomManager.RoomService.Application.Commands;
using RoomManager.RoomService.Application.Ports;
using RoomManager.RoomService.Domain.Exceptions;
using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManager.RoomService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomController> _logger;

        public RoomController(IRoomService roomService, ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active rooms
        /// </summary>
        /// <returns>List of all active rooms</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ServiceResponse<List<RoomDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceResponse<List<RoomDto>>>> GetAllRooms()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync();
                return Ok(ServiceResponse<List<RoomDto>>.SuccessResponse(rooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all rooms");
                return StatusCode(500, ServiceResponse<List<RoomDto>>.ErrorResponse("Internal server error"));
            }
        }

        /// <summary>
        /// Get a specific room by ID
        /// </summary>
        /// <param name="id">The room ID</param>
        /// <returns>The room details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ServiceResponse<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<RoomDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> GetRoomById(Guid id)
        {
            try
            {
                var room = await _roomService.GetRoomByIdAsync(id);
                return Ok(ServiceResponse<RoomDto>.SuccessResponse(room));
            }
            catch (RoomNotFoundException ex)
            {
                return NotFound(ServiceResponse<RoomDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting room {RoomId}", id);
                return StatusCode(500, ServiceResponse<RoomDto>.ErrorResponse("Internal server error"));
            }
        }

        /// <summary>
        /// Get rooms by type
        /// </summary>
        /// <param name="type">The room type (Conference, Classroom, Lab, Study, Meeting, Auditorium, Office)</param>
        /// <returns>List of rooms matching the type</returns>
        [HttpGet("type/{type}")]
        [ProducesResponseType(typeof(ServiceResponse<List<RoomDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<List<RoomDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<List<RoomDto>>>> GetRoomsByType(string type)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByTypeAsync(type);
                return Ok(ServiceResponse<List<RoomDto>>.SuccessResponse(rooms));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ServiceResponse<List<RoomDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting rooms by type {Type}", type);
                return StatusCode(500, ServiceResponse<List<RoomDto>>.ErrorResponse("Internal server error"));
            }
        }

        /// <summary>
        /// Get rooms by minimum capacity
        /// </summary>
        /// <param name="minCapacity">The minimum capacity required</param>
        /// <returns>List of rooms with at least the specified capacity</returns>
        [HttpGet("capacity/{minCapacity:int}")]
        [ProducesResponseType(typeof(ServiceResponse<List<RoomDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<List<RoomDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<List<RoomDto>>>> GetRoomsByCapacity(int minCapacity)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByCapacityAsync(minCapacity);
                return Ok(ServiceResponse<List<RoomDto>>.SuccessResponse(rooms));
            }
            catch (InvalidRoomDataException ex)
            {
                return BadRequest(ServiceResponse<List<RoomDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting rooms by capacity {Capacity}", minCapacity);
                return StatusCode(500, ServiceResponse<List<RoomDto>>.ErrorResponse("Internal server error"));
            }
        }

        /// <summary>
        /// Create a new room
        /// </summary>
        /// <param name="request">The room creation request</param>
        /// <returns>The created room</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ServiceResponse<RoomDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ServiceResponse<RoomDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> CreateRoom([FromBody] CreateRoomRequest request)
        {
            try
            {
                var command = new CreateRoomCommand(request.Name, request.Capacity, request.Type, request.Location, request.Description);
                var room = await _roomService.CreateRoomAsync(command);

                return CreatedAtAction(nameof(GetRoomById),
                    new { id = room.Id },
                    ServiceResponse<RoomDto>.SuccessResponse(room));
            }
            catch (RoomNameAlreadyExistsException ex)
            {
                return BadRequest(ServiceResponse<RoomDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ServiceResponse<RoomDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating room");
                return StatusCode(500, ServiceResponse<RoomDto>.ErrorResponse("Internal server error"));
            }
        }

        /// <summary>
        /// Update an existing room
        /// </summary>
        /// <param name="id">The room ID</param>
        /// <param name="request">The room update request</param>
        /// <returns>The updated room</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ServiceResponse<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<RoomDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServiceResponse<RoomDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> UpdateRoom(Guid id, [FromBody] UpdateRoomRequest request)
        {
            try
            {
                var command = new UpdateRoomCommand(request.Name, request.Capacity, request.Type, request.Location, request.Description);
                var room = await _roomService.UpdateRoomAsync(id, command);

                return Ok(ServiceResponse<RoomDto>.SuccessResponse(room));
            }
            catch (RoomNotFoundException ex)
            {
                return NotFound(ServiceResponse<RoomDto>.ErrorResponse(ex.Message));
            }
            catch (RoomNameAlreadyExistsException ex)
            {
                return BadRequest(ServiceResponse<RoomDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ServiceResponse<RoomDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating room {RoomId}", id);
                return StatusCode(500, ServiceResponse<RoomDto>.ErrorResponse("Internal server error"));
            }
        }

        /// <summary>
        /// Delete a room (soft delete)
        /// </summary>
        /// <param name="id">The room ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<object>>> DeleteRoom(Guid id)
        {
            try
            {
                await _roomService.DeleteRoomAsync(id);
                return Ok(ServiceResponse<object>.SuccessResponse(null));
            }
            catch (RoomNotFoundException ex)
            {
                return NotFound(ServiceResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting room {RoomId}", id);
                return StatusCode(500, ServiceResponse<object>.ErrorResponse("Internal server error"));
            }
        }
    }
}