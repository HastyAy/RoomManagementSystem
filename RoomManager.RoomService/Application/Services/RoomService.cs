using RoomManager.RoomService.Application.Commands;
using RoomManager.RoomService.Application.Ports;
using RoomManager.RoomService.Domain.Entities;
using RoomManager.RoomService.Domain.Exceptions;
using RoomManager.RoomService.Domain.ValueObjects;
using RoomManager.Shared.DTOs.RoomDto;
using System.Xml.Linq;

namespace RoomManager.RoomService.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ILogger<RoomService> _logger;

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

            var roomType = RoomType.Create(type); // Validates type
            var rooms = await _roomRepository.GetByTypeAsync(roomType);
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
            _logger.LogInformation("Creating new room: {Name}", command.Name);

            // Validate uniqueness
            if (!await _roomRepository.IsNameUniqueAsync(command.Name))
            {
                throw new RoomNameAlreadyExistsException(command.Name);
            }

            // Create domain entity
            var roomType = RoomType.Create(command.Type);
            var room = new Room(command.Name, command.Capacity, roomType, command.Location, command.Description);

            // Persist
            var savedRoom = await _roomRepository.AddAsync(room);

            _logger.LogInformation("Room created successfully with ID: {RoomId}", savedRoom.Id);
            return MapToSharedDto(savedRoom);
        }

        public async Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomCommand command)
        {
            _logger.LogInformation("Updating room with ID: {RoomId}", id);

            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null || !room.IsActive)
            {
                throw new RoomNotFoundException(id);
            }

            // Validate uniqueness
            if (!await _roomRepository.IsNameUniqueAsync(command.Name, id))
            {
                throw new RoomNameAlreadyExistsException(command.Name);
            }

            // Update domain entity
            var roomType = RoomType.Create(command.Type);
            room.UpdateDetails(command.Name, command.Capacity, roomType, command.Location, command.Description);

            // Persist
            var updatedRoom = await _roomRepository.UpdateAsync(room);

            _logger.LogInformation("Room updated successfully: {RoomId}", id);
            return MapToSharedDto(updatedRoom);
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

        private static RoomDto MapToSharedDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Type = room.Type, // RoomType wird implizit zu string konvertiert
                Location = room.Location,
                Description = room.Description
            };
        }
    }
}