using Microsoft.AspNetCore.Mvc;
using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.UserDto;
using RoomManager.UserService.Entities;
using RoomManager.UserService.Repositories;

namespace RoomManager.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessorController : ControllerBase
    {
        private readonly IProfessorRepository _repo;

        public ProfessorController(IProfessorRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<ProfessorDto>>>> Get()
        {
            var professors = await _repo.GetActiveProfessorsAsync();
            var professorDtos = professors.Select(p => new ProfessorDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                Department = p.Department,
                Title = p.Title,
                FullName = p.FullName
            }).ToList();

            return Ok(ServiceResponse<List<ProfessorDto>>.SuccessResponse(professorDtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<ProfessorDto>>> GetById(Guid id)
        {
            var professor = await _repo.GetByIdAsync(id);
            if (professor == null || !professor.IsActive)
            {
                return NotFound(ServiceResponse<ProfessorDto>.ErrorResponse("Professor not found"));
            }

            var professorDto = new ProfessorDto
            {
                Id = professor.Id,
                FirstName = professor.FirstName,
                LastName = professor.LastName,
                Email = professor.Email,
                Department = professor.Department,
                Title = professor.Title,
                FullName = professor.FullName
            };

            return Ok(ServiceResponse<ProfessorDto>.SuccessResponse(professorDto));
        }

        [HttpGet("department/{department}")]
        public async Task<ActionResult<ServiceResponse<List<ProfessorDto>>>> GetByDepartment(string department)
        {
            var professors = await _repo.GetByDepartmentAsync(department);
            var professorDtos = professors.Select(p => new ProfessorDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                Department = p.Department,
                Title = p.Title,
                FullName = p.FullName
            }).ToList();

            return Ok(ServiceResponse<List<ProfessorDto>>.SuccessResponse(professorDtos));
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<ProfessorDto>>> Post([FromBody] CreateProfessorRequest request)
        {
            if (!await _repo.IsEmailUniqueAsync(request.Email))
            {
                return BadRequest(ServiceResponse<ProfessorDto>.ErrorResponse("Email already exists"));
            }

            var professor = new Professor
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Department = request.Department,
                Title = request.Title
            };

            await _repo.AddAsync(professor);

            var professorDto = new ProfessorDto
            {
                Id = professor.Id,
                FirstName = professor.FirstName,
                LastName = professor.LastName,
                Email = professor.Email,
                Department = professor.Department,
                Title = professor.Title,
                FullName = professor.FullName
            };

            return CreatedAtAction(nameof(GetById), new { id = professor.Id },
                ServiceResponse<ProfessorDto>.SuccessResponse(professorDto));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<ProfessorDto>>> Put(Guid id, [FromBody] CreateProfessorRequest request)
        {
            var professor = await _repo.GetByIdAsync(id);
            if (professor == null || !professor.IsActive)
            {
                return NotFound(ServiceResponse<ProfessorDto>.ErrorResponse("Professor not found"));
            }

            if (!await _repo.IsEmailUniqueAsync(request.Email, id))
            {
                return BadRequest(ServiceResponse<ProfessorDto>.ErrorResponse("Email already exists"));
            }

            professor.FirstName = request.FirstName;
            professor.LastName = request.LastName;
            professor.Email = request.Email;
            professor.Department = request.Department;
            professor.Title = request.Title;

            await _repo.UpdateAsync(professor);

            var professorDto = new ProfessorDto
            {
                Id = professor.Id,
                FirstName = professor.FirstName,
                LastName = professor.LastName,
                Email = professor.Email,
                Department = professor.Department,
                Title = professor.Title,
                FullName = professor.FullName
            };

            return Ok(ServiceResponse<ProfessorDto>.SuccessResponse(professorDto));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<object>>> Delete(Guid id)
        {
            var professor = await _repo.GetByIdAsync(id);
            if (professor == null || !professor.IsActive)
            {
                return NotFound(ServiceResponse<object>.ErrorResponse("Professor not found"));
            }

            await _repo.DeleteAsync(id);
            return Ok(ServiceResponse<object>.SuccessResponse(null));
        }
    }
}
