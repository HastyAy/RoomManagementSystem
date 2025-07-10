using RoomManager.RoomService.Application.Commands;
using RoomManager.RoomService.Application.Ports;
using RoomManager.RoomService.Domain.Entities;
using RoomManager.RoomService.Domain.Exceptions;
using RoomManager.Shared.DTOs.RoomDto;

namespace RoomManager.RoomService.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ILogger<RoomService> _logger;

        // Valid room types
        private static readonly HashSet<string> ValidTypes = new()
        {
            "Conference", "Classroom", "Lab", "Study", "Meeting", "Auditorium", "Office"
        };

        public RoomService(IRoomRepository roomRepository, ILogger<RoomService> logger)
        {
            _roomRepository = roomRepository;
            _logger = logger;
        }

        public async Task<List<RoomDto>> GetAllRoomsAsync()
        {
            _logger.LogInformation("Getting all active rooms");
            var rooms = await _roomRepository.GetActiveRoomsAsync();
            return rooms.Select(MapToSharedDto).ToList();
        }

        public async Task<RoomDto> GetRoomByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting room with ID: {RoomId}", id);

            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null || !room.IsActive)
            {
                throw new RoomNotFoundException(id);
            }

            return MapToSharedDto(room);
        }

        public async Task<List<RoomDto>> GetRoomsByTypeAsync(string type)
        {
            _logger.LogInformation("Getting rooms by type: {Type}", type);

            if (!IsValidRoomType(type))
            {
                throw new InvalidRoomDataException($"Invalid room type: {type}");
            }

            var rooms = await _roomRepository.GetByTypeAsync(type);
            return rooms.Select(MapToSharedDto).ToList();
        }

        public async Task<List<RoomDto>> GetRoomsByCapacityAsync(int minCapacity)
        {
            _logger.LogInformation("Getting rooms with minimum capacity: {Capacity}", minCapacity);

            if (minCapacity <= 0)
            {
                throw new InvalidRoomDataException("Minimum capacity must be positive");
            }

            var rooms = await _roomRepository.GetByCapacityAsync(minCapacity);
            return rooms.Select(MapToSharedDto).ToList();
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomCommand command)
        {
            _logger.LogInformation("Creating new room: {Name} with type: {Type}", command.Name, command.Type);

            try
            {
                // Validate inputs
                ValidateRoomCommand(command);

                // Validate uniqueness
                if (!await _roomRepository.IsNameUniqueAsync(command.Name))
                {
                    _logger.LogWarning("Room name already exists: {Name}", command.Name);
                    throw new RoomNameAlreadyExistsException(command.Name);
                }

                var room = new Room(
                    name: command.Name,
                    capacity: command.Capacity,
                    type: command.Type, // Direct string assignment
                    location: command.Location,
                    description: command.Description
                );

                // Persist
                var savedRoom = await _roomRepository.AddAsync(room);

                _logger.LogInformation("Room created successfully with ID: {RoomId}", savedRoom.Id);
                return MapToSharedDto(savedRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create room: {Name}", command.Name);
                throw;
            }
        }

        public async Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomCommand command)
        {
            _logger.LogInformation("Updating room with ID: {RoomId}", id);

            try
            {
                var room = await _roomRepository.GetByIdAsync(id);
                if (room == null || !room.IsActive)
                {
                    throw new RoomNotFoundException(id);
                }

                // Validate inputs
                ValidateRoomCommand(command);

                // Validate uniqueness
                if (!await _roomRepository.IsNameUniqueAsync(command.Name, id))
                {
                    throw new RoomNameAlreadyExistsException(command.Name);
                }

                // Update domain entity - DIRECTLY with string type
                room.UpdateDetails(
                    name: command.Name,
                    capacity: command.Capacity,
                    type: command.Type, // Direct string assignment
                    location: command.Location,
                    description: command.Description
                );

                // Persist
                var updatedRoom = await _roomRepository.UpdateAsync(room);

                _logger.LogInformation("Room updated successfully: {RoomId}", id);
                return MapToSharedDto(updatedRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update room: {RoomId}", id);
                throw;
            }
        }

        public async Task DeleteRoomAsync(Guid id)
        {
            _logger.LogInformation("Deleting room with ID: {RoomId}", id);

            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null || !room.IsActive)
            {
                throw new RoomNotFoundException(id);
            }

            await _roomRepository.DeleteAsync(id);
            _logger.LogInformation("Room deleted successfully: {RoomId}", id);
        }

        private static void ValidateRoomCommand(dynamic command)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                throw new InvalidRoomDataException("Room name is required");

            if (command.Capacity <= 0)
                throw new InvalidRoomDataException("Room capacity must be greater than 0");

            if (!IsValidRoomType(command.Type))
                throw new InvalidRoomDataException($"Invalid room type: {command.Type}. Valid types: {string.Join(", ", ValidTypes)}");
        }

        private static bool IsValidRoomType(string type)
        {
            return !string.IsNullOrWhiteSpace(type) && ValidTypes.Contains(type);
        }

        private static RoomDto MapToSharedDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Type = room.Type,
                Location = room.Location,
                Description = room.Description
            };
        }
    }
}