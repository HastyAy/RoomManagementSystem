using RoomManagement.Frontend.Services;
using RoomManager.Shared.DTOs.RoomDto;

public class RoomServiceWithRetry
{
    // Retry Policy für robustere Kommunikation
    private readonly RoomService _roomService;
    private readonly ILogger<RoomServiceWithRetry> _logger;
    private const int MaxRetries = 3;

    public RoomServiceWithRetry(RoomService roomService, ILogger<RoomServiceWithRetry> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    public async Task<List<RoomDto>> GetAllWithRetryAsync()
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await _roomService.GetAllAsync();
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning("Attempt {Attempt} failed: {Error}. Retrying...", attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
            }
        }

        _logger.LogError("All {MaxRetries} attempts failed", MaxRetries);
        return new List<RoomDto>();
    }
}