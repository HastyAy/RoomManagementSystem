using System.Net.Http.Json;
using RoomManager.Shared.Entities;

namespace RoomManager.Frontend.Services;

public class StudentService
{
    private readonly HttpClient _http;

    public StudentService(HttpClient http) => _http = http;

    public async Task<List<Student>> GetAllAsync() =>
      await _http.GetFromJsonAsync<List<Student>>("api/student") ?? new();

    public async Task AddAsync(Student student) =>
        await _http.PostAsJsonAsync("api/student", student);

    public async Task UpdateAsync(Student student) =>
        await _http.PutAsJsonAsync($"api/student/{student.Id}", student);

    public async Task DeleteAsync(Guid id) =>
        await _http.DeleteAsync($"api/student/{id}");

}
