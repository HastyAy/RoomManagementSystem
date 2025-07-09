using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.UserDto;
using System.Text.Json;

namespace RoomManager.BookingService.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(IHttpClientFactory httpClientFactory, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("UserService");
            _logger = logger;
        }

        public async Task<StudentDto?> GetStudentAsync(Guid studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/{studentId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<StudentDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return serviceResponse?.Data;
                }

                _logger.LogWarning("Failed to get student {StudentId}. Status: {Status}", studentId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student {StudentId} from UserService", studentId);
                return null;
            }
        }

        public async Task<ProfessorDto?> GetProfessorAsync(Guid professorId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/professor/{professorId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<ProfessorDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return serviceResponse?.Data;
                }

                _logger.LogWarning("Failed to get professor {ProfessorId}. Status: {Status}", professorId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting professor {ProfessorId} from UserService", professorId);
                return null;
            }
        }

        public async Task<bool> StudentExistsAsync(Guid studentId)
        {
            var student = await GetStudentAsync(studentId);
            return student != null;
        }

        public async Task<bool> ProfessorExistsAsync(Guid professorId)
        {
            var professor = await GetProfessorAsync(professorId);
            return professor != null;
        }
    }
}
