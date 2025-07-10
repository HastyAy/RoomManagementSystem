using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.RoomDto;
using System.Net;
using System.Text.Json;

namespace RoomManagement.Frontend.Services
{
    public class RoomService
    {
        private readonly HttpClient _http;
        private readonly ILogger<RoomService> _logger;

        public RoomService(HttpClient http, ILogger<RoomService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // Original Methoden bleiben kompatibel
        public async Task<List<RoomDto>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ServiceResponse<List<RoomDto>>>("api/room");
                return response?.Data ?? new List<RoomDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve rooms");
                return new List<RoomDto>();
            }
        }

        // Erweiterte Versionen mit besserer Error Handling
        public async Task<(List<RoomDto> Rooms, string? ErrorMessage)> GetAllWithErrorAsync()
        {
            try
            {
                var httpResponse = await _http.GetAsync("api/room");

                if (httpResponse.IsSuccessStatusCode)
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<List<RoomDto>>>();
                    return (response?.Data ?? new List<RoomDto>(), null);
                }
                else
                {
                    var errorResponse = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<List<RoomDto>>>();
                    return (new List<RoomDto>(), errorResponse?.Message ?? "Unknown error occurred");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while retrieving rooms");
                return (new List<RoomDto>(), "Network error occurred");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse response");
                return (new List<RoomDto>(), "Invalid response format");
            }
        }

        public async Task<RoomDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ServiceResponse<RoomDto>>($"api/room/{id}");
                return response?.Data;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                _logger.LogWarning("Room with ID {RoomId} not found", id);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve room {RoomId}", id);
                return null;
            }
        }

        public async Task<(RoomDto? Room, string? ErrorMessage)> GetByIdWithErrorAsync(Guid id)
        {
            try
            {
                var httpResponse = await _http.GetAsync($"api/room/{id}");

                if (httpResponse.IsSuccessStatusCode)
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<RoomDto>>();
                    return (response?.Data, null);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    var errorResponse = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<RoomDto>>();
                    return (null, errorResponse?.Message ?? "Room not found");
                }
                else
                {
                    var errorResponse = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<RoomDto>>();
                    return (null, errorResponse?.Message ?? "Error retrieving room");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room {RoomId}", id);
                return (null, "An unexpected error occurred");
            }
        }

        // Neue Methoden für erweiterte Funktionalität
        public async Task<List<RoomDto>> GetByTypeAsync(string type)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ServiceResponse<List<RoomDto>>>($"api/room/type/{type}");
                return response?.Data ?? new List<RoomDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve rooms by type {Type}", type);
                return new List<RoomDto>();
            }
        }

        public async Task<List<RoomDto>> GetByCapacityAsync(int minCapacity)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ServiceResponse<List<RoomDto>>>($"api/room/capacity/{minCapacity}");
                return response?.Data ?? new List<RoomDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve rooms by capacity {Capacity}", minCapacity);
                return new List<RoomDto>();
            }
        }

        public async Task<bool> AddAsync(CreateRoomRequest room)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/room", room);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to create room {RoomName}", room.Name);
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> AddWithErrorAsync(CreateRoomRequest room)
        {
            try
            {
                var httpResponse = await _http.PostAsJsonAsync("api/room", room);

                if (httpResponse.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    var errorResponse = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<RoomDto>>();
                    return (false, errorResponse?.Message ?? "Failed to create room");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room {RoomName}", room.Name);
                return (false, "An unexpected error occurred");
            }
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateRoomRequest room)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/room/{id}", room);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to update room {RoomId}", id);
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateWithErrorAsync(Guid id, UpdateRoomRequest room)
        {
            try
            {
                var httpResponse = await _http.PutAsJsonAsync($"api/room/{id}", room);

                if (httpResponse.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    var errorResponse = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<RoomDto>>();
                    return (false, errorResponse?.Message ?? "Failed to update room");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {RoomId}", id);
                return (false, "An unexpected error occurred");
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/room/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to delete room {RoomId}", id);
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteWithErrorAsync(Guid id)
        {
            try
            {
                var httpResponse = await _http.DeleteAsync($"api/room/{id}");

                if (httpResponse.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return (false, "Room not found");
                }
                else
                {
                    var errorResponse = await httpResponse.Content.ReadFromJsonAsync<ServiceResponse<object>>();
                    return (false, errorResponse?.Message ?? "Failed to delete room");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {RoomId}", id);
                return (false, "An unexpected error occurred");
            }
        }

        // Backward compatibility aliases
        public async Task<List<RoomDto>> GetRoomsAsync() => await GetAllAsync();
        public async Task<bool> AddRoomAsync(CreateRoomRequest room) => await AddAsync(room);
        public async Task<bool> UpdateRoomAsync(Guid id, UpdateRoomRequest room) => await UpdateAsync(id, room);
        public async Task<bool> DeleteRoomAsync(Guid id) => await DeleteAsync(id);

        // Validation Helpers
        public bool IsValidRoomType(string type)
        {
            var validTypes = new[] { "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium", "Office" };
            return validTypes.Contains(type);
        }

        public List<string> GetValidRoomTypes()
        {
            return new List<string> { "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium", "Office" };
        }
    }
}