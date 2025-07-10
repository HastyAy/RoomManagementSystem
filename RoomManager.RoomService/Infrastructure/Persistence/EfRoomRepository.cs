using Microsoft.EntityFrameworkCore;
using RoomManager.RoomService.Application.Ports;
using RoomManager.RoomService.Domain.Entities;

namespace RoomManager.RoomService.Infrastructure.Persistence
{
    public class EfRoomRepository : IRoomRepository
    {
        private readonly RoomDbContext _context;

        public EfRoomRepository(RoomDbContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAllAsync()
        {
            return await _context.Rooms.OrderBy(r => r.Name).ToListAsync();
        }

        public async Task<List<Room>> GetActiveRoomsAsync()
        {
            return await _context.Rooms
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Room>> GetByTypeAsync(string type)
        {
            return await _context.Rooms
                .Where(r => r.Type == type && r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<Room>> GetByCapacityAsync(int minCapacity)
        {
            return await _context.Rooms
                .Where(r => r.Capacity >= minCapacity && r.IsActive)
                .OrderBy(r => r.Capacity)
                .ToListAsync();
        }

        public async Task<Room> AddAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room> UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task DeleteAsync(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                room.Deactivate();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Rooms.AnyAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Rooms.Where(r => r.Name == name && r.IsActive);
            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }
    }
}