using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.BookingDto;

namespace RoomManagement.Frontend.Services
{
    public class BookingService
    {
        private readonly HttpClient _http;

        public BookingService(HttpClient http) => _http = http;

        public async Task<List<BookingDto>> GetAllAsync()
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<BookingDto>>>("api/booking");
            return response?.Data ?? new List<BookingDto>();
        }

        public async Task<BookingDto?> GetByIdAsync(Guid id)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<BookingDto>>($"api/booking/{id}");
            return response?.Data;
        }

        public async Task<List<BookingDto>> GetByRoomIdAsync(Guid roomId)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<BookingDto>>>($"api/booking/room/{roomId}");
            return response?.Data ?? new List<BookingDto>();
        }

        public async Task<List<BookingDto>> GetByStudentIdAsync(Guid studentId)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<BookingDto>>>($"api/booking/student/{studentId}");
            return response?.Data ?? new List<BookingDto>();
        }

        public async Task<List<BookingDto>> GetByProfessorIdAsync(Guid professorId)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<BookingDto>>>($"api/booking/professor/{professorId}");
            return response?.Data ?? new List<BookingDto>();
        }

        public async Task<bool> AddAsync(CreateBookingRequest booking)
        {
            var response = await _http.PostAsJsonAsync("api/booking", booking);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateBookingRequest booking)
        {
            var response = await _http.PutAsJsonAsync($"api/booking/{id}", booking);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/booking/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckRoomAvailabilityAsync(Guid roomId, DateTime startTime, DateTime endTime)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<bool>>(
                $"api/booking/room/{roomId}/availability?startTime={startTime:O}&endTime={endTime:O}");
            return response?.Data ?? false;
        }
    }
}