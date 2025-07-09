using System.Net.Http.Json;
using RoomManager.Shared.Entities;

namespace RoomManagement.Frontend.Services;

public class BookingService
{
    private readonly HttpClient _http;

    public BookingService(HttpClient http) => _http = http;

    public async Task<List<Booking>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<Booking>>("api/booking") ?? new();

    public async Task<Booking?> GetByIdAsync(Guid id) =>
        await _http.GetFromJsonAsync<Booking>($"api/booking/{id}");

    public async Task AddAsync(Booking booking) =>
        await _http.PostAsJsonAsync("api/booking", booking);

    public async Task UpdateAsync(Booking booking) =>
        await _http.PutAsJsonAsync($"api/booking/{booking.Id}", booking);

    public async Task DeleteAsync(Guid id) =>
        await _http.DeleteAsync($"api/booking/{id}");
}
