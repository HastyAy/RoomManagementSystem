using Microsoft.EntityFrameworkCore;
using RoomManager.BookingService.Context;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;

namespace RoomManager.BookingService.Repositories
{
    public class ProfessorRepository : IProfessorRepository
    {
        private readonly BookingDbContext _context;

        public ProfessorRepository(BookingDbContext context) => _context = context;

        public async Task<List<Professor>> GetAllAsync() => await _context.Professors.ToListAsync();

        public async Task<Professor?> GetByIdAsync(Guid id) => await _context.Professors.FindAsync(id);

        public async Task AddAsync(Professor professor)
        {
            professor.Id = Guid.NewGuid();
            _context.Professors.Add(professor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Professor professor)
        {
            _context.Professors.Update(professor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var professor = await _context.Professors.FindAsync(id);
            if (professor != null)
            {
                _context.Professors.Remove(professor);
                await _context.SaveChangesAsync();
            }
        }
    }

}
