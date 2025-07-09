using System.Net.Http.Json;
using RoomManager.Shared.Entities;

namespace RoomManagement.Frontend.Services;

public class ProfessorService
{
    private readonly HttpClient _http;

    public ProfessorService(HttpClient http) => _http = http;

    public async Task<List<Professor>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<Professor>>("api/professor") ?? new();

    public async Task<Professor?> GetByIdAsync(Guid id) =>
        await _http.GetFromJsonAsync<Professor>($"api/professor/{id}");

    public async Task AddAsync(Professor professor) =>
        await _http.PostAsJsonAsync("api/professor", professor);

    public async Task UpdateAsync(Professor professor) =>
        await _http.PutAsJsonAsync($"api/professor/{professor.Id}", professor);

    public async Task DeleteAsync(Guid id) =>
        await _http.DeleteAsync($"api/professor/{id}");
}
