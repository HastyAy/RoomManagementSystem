using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManagement.Frontend.Services
{
    public class RoomService
    {
        private readonly HttpClient _http;

        public RoomService(HttpClient http) => _http = http;

        public async Task<List<RoomDto>> GetAllAsync()
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<RoomDto>>>("api/room");
            return response?.Data ?? new List<RoomDto>();
        }

        public async Task<List<RoomDto>> GetRoomsAsync() => await GetAllAsync();

        public async Task<RoomDto?> GetByIdAsync(Guid id)
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<RoomDto>>($"api/room/{id}");
            return response?.Data;
        }

        public async Task<bool> AddAsync(CreateRoomRequest room)
        {
            var response = await _http.PostAsJsonAsync("api/room", room);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddRoomAsync(CreateRoomRequest room) => await AddAsync(room);

        public async Task<bool> UpdateAsync(Guid id, UpdateRoomRequest room)
        {
            var response = await _http.PutAsJsonAsync($"api/room/{id}", room);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRoomAsync(Guid id, UpdateRoomRequest room) => await UpdateAsync(id, room);

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/room/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoomAsync(Guid id) => await DeleteAsync(id);
    }
}