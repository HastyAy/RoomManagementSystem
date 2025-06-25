using RoomManager.Shared.Entities;

namespace RoomManager.Shared.Repositories;

public interface IProfessorRepository
{
    Task<List<Professor>> GetAllAsync();
    Task<Professor?> GetByIdAsync(Guid id);
    Task AddAsync(Professor professor);
    Task UpdateAsync(Professor professor);
    Task DeleteAsync(Guid id);
}
