using Microsoft.AspNetCore.Mvc;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;

namespace RoomManager.BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentRepository _repo;
    public StudentController(IStudentRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> Get() => await _repo.GetAllAsync();

    [HttpGet("{id}")]  
    public async Task<ActionResult<Student>> GetById(Guid id)
    {
        var student = await _repo.GetByIdAsync(id);
        return student == null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Student student)
    {
        await _repo.AddAsync(student);
        return Ok(student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] Student student)
    {
        if (id != student.Id) return BadRequest();
        await _repo.UpdateAsync(student);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}