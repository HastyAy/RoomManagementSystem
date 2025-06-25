using System.Net.Http.Json;
using RoomManager.Shared.Entities;

namespace RoomManager.Frontend.Services;

public class RoomService
{
    private readonly HttpClient _http;

    public RoomService(HttpClient http) => _http = http;

    public async Task<List<Room>> GetRoomsAsync() =>
        await _http.GetFromJsonAsync<List<Room>>("api/room") ?? new();

    public async Task AddRoomAsync(Room room) =>
        await _http.PostAsJsonAsync("api/room", room);

    public async Task UpdateRoomAsync(Room room) =>
        await _http.PutAsJsonAsync($"api/room/{room.Id}", room);

    public async Task DeleteRoomAsync(Guid id) =>
        await _http.DeleteAsync($"api/room/{id}");
}

