using Microsoft.EntityFrameworkCore;
using RoomManager.UserService.Context;
using RoomManager.UserService.Entities;

namespace RoomManager.UserService.Repositories
{
    public class ProfessorRepository : IProfessorRepository
    {
        private readonly UserDbContext _context;

        public ProfessorRepository(UserDbContext context) => _context = context;

        public async Task<List<Professor>> GetAllAsync()
        {
            return await _context.Professors.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync();
        }

        public async Task<List<Professor>> GetActiveProfessorsAsync()
        {
            return await _context.Professors
                .Where(p => p.IsActive)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        public async Task<Professor?> GetByIdAsync(Guid id)
        {
            return await _context.Professors.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Professor?> GetByEmailAsync(string email)
        {
            return await _context.Professors.FirstOrDefaultAsync(p => p.Email == email && p.IsActive);
        }

        public async Task<List<Professor>> GetByDepartmentAsync(string department)
        {
            return await _context.Professors
                .Where(p => p.Department == department && p.IsActive)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        public async Task AddAsync(Professor professor)
        {
            professor.Id = Guid.NewGuid();
            professor.CreatedAt = DateTime.UtcNow;
            _context.Professors.Add(professor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Professor professor)
        {
            professor.UpdatedAt = DateTime.UtcNow;
            _context.Professors.Update(professor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var professor = await _context.Professors.FindAsync(id);
            if (professor != null)
            {
                professor.IsActive = false;
                professor.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Professors.AnyAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
        {
            var query = _context.Professors.Where(p => p.Email == email);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }
    }
}
