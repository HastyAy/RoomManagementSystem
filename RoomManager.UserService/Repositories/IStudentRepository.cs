using RoomManager.UserService.Entities;

namespace RoomManager.UserService.Repositories
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllAsync();
        Task<List<Student>> GetActiveStudentsAsync();
        Task<Student?> GetByIdAsync(Guid id);
        Task<Student?> GetByMatriNumberAsync(string matriNumber);
        Task<Student?> GetByEmailAsync(string email);
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsMatriNumberUniqueAsync(string matriNumber, Guid? excludeId = null);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    }
}
