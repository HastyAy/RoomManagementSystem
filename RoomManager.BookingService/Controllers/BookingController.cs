using Microsoft.AspNetCore.Mvc;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;

namespace RoomManager.BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _repo;
        public RoomController(IRoomRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> Get() => await _repo.GetAllAsync();
    }

}
