using Microsoft.AspNetCore.Mvc;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;

namespace RoomManager.BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _repo;
        public BookingController(IBookingRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> Get() => await _repo.GetAllAsync();

        [HttpGet("{id}")]  
        public async Task<ActionResult<Booking>> GetById(Guid id)
        {
            var booking = await _repo.GetByIdAsync(id);
            return booking == null ? NotFound() : Ok(booking);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Booking booking)
        {
            await _repo.AddAsync(booking);
            return Ok(booking);
        }

        [HttpPut("{id}")]  
        public async Task<ActionResult> Put(Guid id, [FromBody] Booking booking)
        {
            if (id != booking.Id) return BadRequest();
            await _repo.UpdateAsync(booking);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}