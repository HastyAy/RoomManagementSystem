using RoomManager.UserService.Entities;

namespace RoomManager.UserService.Repositories
{
    public interface IProfessorRepository
    {
        Task<List<Professor>> GetAllAsync();
        Task<List<Professor>> GetActiveProfessorsAsync();
        Task<Professor?> GetByIdAsync(Guid id);
        Task<Professor?> GetByEmailAsync(string email);
        Task<List<Professor>> GetByDepartmentAsync(string department);
        Task AddAsync(Professor professor);
        Task UpdateAsync(Professor professor);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    }
}
