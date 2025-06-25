using System.Net.Http.Json;
using RoomManager.Shared.Entities;

namespace RoomManager.Frontend.Services;

public class BookingService
{
    private readonly HttpClient _http;

    public BookingService(HttpClient http) => _http = http;

    public async Task<List<Booking>> GetAllAsync() =>
     await _http.GetFromJsonAsync<List<Booking>>("api/booking") ?? new();

    public async Task AddAsync(Booking booking) =>
        await _http.PostAsJsonAsync("api/booking", booking);

    public async Task DeleteAsync(Guid id) =>
        await _http.DeleteAsync($"api/booking/{id}");

}
