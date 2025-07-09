using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.RoomDto;
using System.Text.Json;

namespace RoomManager.BookingService.Services
{
    public class RoomServiceClient : IRoomServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RoomServiceClient> _logger;

        public RoomServiceClient(IHttpClientFactory httpClientFactory, ILogger<RoomServiceClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("RoomService");
            _logger = logger;
        }

        public async Task<RoomDto?> GetRoomAsync(Guid roomId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/room/{roomId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<RoomDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return serviceResponse?.Data;
                }

                _logger.LogWarning("Failed to get room {RoomId}. Status: {Status}", roomId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room {RoomId} from RoomService", roomId);
                return null;
            }
        }

        public async Task<bool> RoomExistsAsync(Guid roomId)
        {
            var room = await GetRoomAsync(roomId);
            return room != null;
        }

        public async Task<List<RoomDto>> GetAvailableRoomsAsync(int minCapacity, string? type = null)
        {
            try
            {
                var url = $"api/room/capacity/{minCapacity}";
                if (!string.IsNullOrEmpty(type))
                {
                    url = $"api/room/type/{type}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<List<RoomDto>>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return serviceResponse?.Data ?? new List<RoomDto>();
                }

                return new List<RoomDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available rooms from RoomService");
                return new List<RoomDto>();
            }
        }
    }
}
