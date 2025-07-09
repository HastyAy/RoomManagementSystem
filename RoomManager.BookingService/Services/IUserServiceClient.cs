using RoomManager.Shared.DTOs.UserDto;

namespace RoomManager.BookingService.Services
{
    public interface IUserServiceClient
    {
        Task<StudentDto?> GetStudentAsync(Guid studentId);
        Task<ProfessorDto?> GetProfessorAsync(Guid professorId);
        Task<bool> StudentExistsAsync(Guid studentId);
        Task<bool> ProfessorExistsAsync(Guid professorId);
    }
}
