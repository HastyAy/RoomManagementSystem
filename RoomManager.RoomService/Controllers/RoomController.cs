using Microsoft.AspNetCore.Mvc;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;

namespace RoomManager.RoomService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomRepository _repo;

    public RoomController(IRoomRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetAll() => await _repo.GetAllAsync();

    [HttpPost]
    public async Task<ActionResult> Create(Room room)
    {
        await _repo.AddAsync(room);
        return Ok(room);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetById(Guid id)
    {
        var room = await _repo.GetByIdAsync(id);
        return room == null ? NotFound() : Ok(room);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, Room room)
    {
        if (id != room.Id) return BadRequest();
        await _repo.UpdateAsync(room);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
