using Microsoft.AspNetCore.Mvc;
using RoomManager.BookingService.Services;
using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.BookingDto;

namespace RoomManager.BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingBusinessService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingBusinessService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<BookingDto>>>> Get()
        {
            _logger.LogInformation("Getting all bookings");
            var result = await _bookingService.GetAllBookingsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<BookingDto>>> GetById(Guid id)
        {
            _logger.LogInformation("Getting booking {BookingId}", id);
            var result = await _bookingService.GetBookingAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("room/{roomId}")]
        public async Task<ActionResult<ServiceResponse<List<BookingDto>>>> GetByRoomId(Guid roomId)
        {
            _logger.LogInformation("Getting bookings for room {RoomId}", roomId);
            var result = await _bookingService.GetBookingsByRoomAsync(roomId);
            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<ServiceResponse<List<BookingDto>>>> GetByStudentId(Guid studentId)
        {
            _logger.LogInformation("Getting bookings for student {StudentId}", studentId);
            var result = await _bookingService.GetBookingsByUserAsync(studentId, null);
            return Ok(result);
        }

        [HttpGet("professor/{professorId}")]
        public async Task<ActionResult<ServiceResponse<List<BookingDto>>>> GetByProfessorId(Guid professorId)
        {
            _logger.LogInformation("Getting bookings for professor {ProfessorId}", professorId);
            var result = await _bookingService.GetBookingsByUserAsync(null, professorId);
            return Ok(result);
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<ServiceResponse<List<BookingDto>>>> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Getting bookings from {StartDate} to {EndDate}", startDate, endDate);
            var result = await _bookingService.GetBookingsByDateRangeAsync(startDate, endDate);
            return Ok(result);
        }

        [HttpGet("room/{roomId}/availability")]
        public async Task<ActionResult<ServiceResponse<bool>>> CheckRoomAvailability(
            Guid roomId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            _logger.LogInformation("Checking availability for room {RoomId} from {StartTime} to {EndTime}",
                roomId, startTime, endTime);

            var result = await _bookingService.CheckRoomAvailabilityAsync(roomId, startTime, endTime);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<BookingDto>>> Post([FromBody] CreateBookingRequest request)
        {
            _logger.LogInformation("Creating new booking for room {RoomId}", request.RoomId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ServiceResponse<BookingDto>.ErrorResponse(errors));
            }

            var result = await _bookingService.CreateBookingAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<BookingDto>>> Put(Guid id, [FromBody] UpdateBookingRequest request)
        {
            _logger.LogInformation("Updating booking {BookingId}", id);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ServiceResponse<BookingDto>.ErrorResponse(errors));
            }

            var result = await _bookingService.UpdateBookingAsync(id, request);

            if (!result.Success)
            {
                if (result.Message == "Booking not found")
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<object>>> Delete(Guid id)
        {
            _logger.LogInformation("Deleting booking {BookingId}", id);

            var result = await _bookingService.DeleteBookingAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}