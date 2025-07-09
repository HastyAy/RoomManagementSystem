using Microsoft.AspNetCore.Mvc;
using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.UserDto;
using RoomManager.UserService.Entities;
using RoomManager.UserService.Repositories;

namespace RoomManager.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _repo;

        public StudentController(IStudentRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<StudentDto>>>> Get()
        {
            var students = await _repo.GetActiveStudentsAsync();
            var studentDtos = students.Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                MatriNumber = s.MatriNumber,
                Email = s.Email,
                FullName = s.FullName
            }).ToList();

            return Ok(ServiceResponse<List<StudentDto>>.SuccessResponse(studentDtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<StudentDto>>> GetById(Guid id)
        {
            var student = await _repo.GetByIdAsync(id);
            if (student == null || !student.IsActive)
            {
                return NotFound(ServiceResponse<StudentDto>.ErrorResponse("Student not found"));
            }

            var studentDto = new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                MatriNumber = student.MatriNumber,
                Email = student.Email,
                FullName = student.FullName
            };

            return Ok(ServiceResponse<StudentDto>.SuccessResponse(studentDto));
        }

        [HttpGet("matri/{matriNumber}")]
        public async Task<ActionResult<ServiceResponse<StudentDto>>> GetByMatriNumber(string matriNumber)
        {
            var student = await _repo.GetByMatriNumberAsync(matriNumber);
            if (student == null)
            {
                return NotFound(ServiceResponse<StudentDto>.ErrorResponse("Student not found"));
            }

            var studentDto = new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                MatriNumber = student.MatriNumber,
                Email = student.Email,
                FullName = student.FullName
            };

            return Ok(ServiceResponse<StudentDto>.SuccessResponse(studentDto));
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<StudentDto>>> Post([FromBody] CreateStudentRequest request)
        {
            // Validate unique constraints
            if (!await _repo.IsMatriNumberUniqueAsync(request.MatriNumber))
            {
                return BadRequest(ServiceResponse<StudentDto>.ErrorResponse("Matriculation number already exists"));
            }

            if (!await _repo.IsEmailUniqueAsync(request.Email))
            {
                return BadRequest(ServiceResponse<StudentDto>.ErrorResponse("Email already exists"));
            }

            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MatriNumber = request.MatriNumber,
                Email = request.Email
            };

            await _repo.AddAsync(student);

            var studentDto = new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                MatriNumber = student.MatriNumber,
                Email = student.Email,
                FullName = student.FullName
            };

            return CreatedAtAction(nameof(GetById), new { id = student.Id },
                ServiceResponse<StudentDto>.SuccessResponse(studentDto));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<StudentDto>>> Put(Guid id, [FromBody] CreateStudentRequest request)
        {
            var student = await _repo.GetByIdAsync(id);
            if (student == null || !student.IsActive)
            {
                return NotFound(ServiceResponse<StudentDto>.ErrorResponse("Student not found"));
            }

            // Validate unique constraints
            if (!await _repo.IsMatriNumberUniqueAsync(request.MatriNumber, id))
            {
                return BadRequest(ServiceResponse<StudentDto>.ErrorResponse("Matriculation number already exists"));
            }

            if (!await _repo.IsEmailUniqueAsync(request.Email, id))
            {
                return BadRequest(ServiceResponse<StudentDto>.ErrorResponse("Email already exists"));
            }

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.MatriNumber = request.MatriNumber;
            student.Email = request.Email;

            await _repo.UpdateAsync(student);

            var studentDto = new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                MatriNumber = student.MatriNumber,
                Email = student.Email,
                FullName = student.FullName
            };

            return Ok(ServiceResponse<StudentDto>.SuccessResponse(studentDto));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<object>>> Delete(Guid id)
        {
            var student = await _repo.GetByIdAsync(id);
            if (student == null || !student.IsActive)
            {
                return NotFound(ServiceResponse<object>.ErrorResponse("Student not found"));
            }

            await _repo.DeleteAsync(id);
            return Ok(ServiceResponse<object>.SuccessResponse(null));
        }
    }
}
