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

    [HttpPost]
    public async Task<IActionResult> Post(Student student)
    {
        await _repo.AddAsync(student);
        return CreatedAtAction(nameof(Get), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, Student student)
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
