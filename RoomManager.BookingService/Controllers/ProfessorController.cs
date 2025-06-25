using Microsoft.AspNetCore.Mvc;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;

namespace RoomManager.BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessorController : ControllerBase
    {
        private readonly IProfessorRepository _repo;
        public ProfessorController(IProfessorRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Professor>>> Get() => await _repo.GetAllAsync();

        [HttpPost]
        public async Task<IActionResult> Post(Professor prof)
        {
            await _repo.AddAsync(prof);
            return CreatedAtAction(nameof(Get), new { id = prof.Id }, prof);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, Professor prof)
        {
            if (id != prof.Id) return BadRequest();
            await _repo.UpdateAsync(prof);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }

}
