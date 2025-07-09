using Microsoft.EntityFrameworkCore;
using RoomManager.RoomService.Context;
using RoomManager.RoomService.Entities;

namespace RoomManager.RoomService.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly RoomDbContext _context;

    public RoomRepository(RoomDbContext context) => _context = context;

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

    public async Task AddAsync(Room room)
    {
        room.Id = Guid.NewGuid();
        room.CreatedAt = DateTime.UtcNow;
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        room.UpdatedAt = DateTime.UtcNow;
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            room.IsActive = false;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Rooms.AnyAsync(r => r.Id == id && r.IsActive);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _context.Rooms.Where(r => r.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }
}

