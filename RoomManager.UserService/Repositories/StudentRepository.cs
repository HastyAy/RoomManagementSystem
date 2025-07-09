using Microsoft.EntityFrameworkCore;
using RoomManager.UserService.Context;
using RoomManager.UserService.Entities;

namespace RoomManager.UserService.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly UserDbContext _context;

        public StudentRepository(UserDbContext context) => _context = context;

        public async Task<List<Student>> GetAllAsync()
        {
            return await _context.Students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync();
        }

        public async Task<List<Student>> GetActiveStudentsAsync()
        {
            return await _context.Students
                .Where(s => s.IsActive)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(Guid id)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student?> GetByMatriNumberAsync(string matriNumber)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.MatriNumber == matriNumber && s.IsActive);
        }

        public async Task<Student?> GetByEmailAsync(string email)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Email == email && s.IsActive);
        }

        public async Task AddAsync(Student student)
        {
            student.Id = Guid.NewGuid();
            student.CreatedAt = DateTime.UtcNow;
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            student.UpdatedAt = DateTime.UtcNow;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                student.IsActive = false;
                student.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Students.AnyAsync(s => s.Id == id && s.IsActive);
        }

        public async Task<bool> IsMatriNumberUniqueAsync(string matriNumber, Guid? excludeId = null)
        {
            var query = _context.Students.Where(s => s.MatriNumber == matriNumber);
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
        {
            var query = _context.Students.Where(s => s.Email == email);
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }
    }
}
