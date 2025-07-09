using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.UserDto;

namespace RoomManagement.Frontend.Services
{
    public class ProfessorService
    {
        private readonly HttpClient _http;

        public ProfessorService(HttpClient http) => _http = http;

        public async Task<List<ProfessorDto>> GetAllAsync()
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<ProfessorDto>>>("api/professor");
            return response?.Data ?? new List<ProfessorDto>();
        }

        public async Task<ProfessorDto?> GetByIdAsync(Guid id)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<ProfessorDto>>($"api/professor/{id}");
            return response?.Data;
        }

        public async Task<bool> AddAsync(CreateProfessorRequest professor)
        {
            var response = await _http.PostAsJsonAsync("api/professor", professor);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, CreateProfessorRequest professor)
        {
            var response = await _http.PutAsJsonAsync($"api/professor/{id}", professor);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/professor/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}