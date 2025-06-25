using RoomManager.Shared.Entities;

namespace RoomManager.Shared.Repositories;

public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(Guid id);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Guid id);
}
