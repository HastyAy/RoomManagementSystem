using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.UserDto;

namespace RoomManagement.Frontend.Services
{
    public class StudentService
    {
        private readonly HttpClient _http;

        public StudentService(HttpClient http) => _http = http;

        public async Task<List<StudentDto>> GetAllAsync()
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<StudentDto>>>("api/student");
            return response?.Data ?? new List<StudentDto>();
        }

        public async Task<StudentDto?> GetByIdAsync(Guid id)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<StudentDto>>($"api/student/{id}");
            return response?.Data;
        }

        public async Task<bool> AddAsync(CreateStudentRequest student)
        {
            var response = await _http.PostAsJsonAsync("api/student", student);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, CreateStudentRequest student)
        {
            var response = await _http.PutAsJsonAsync($"api/student/{id}", student);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/student/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}