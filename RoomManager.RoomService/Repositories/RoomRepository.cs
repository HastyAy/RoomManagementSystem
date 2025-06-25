using Microsoft.EntityFrameworkCore;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;
using RoomManager.RoomService;
using System;
using RoomManager.RoomService.Context;

namespace RoomManager.RoomService.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly RoomDbContext _context;

    public RoomRepository(RoomDbContext context) => _context = context;

    public async Task<List<Room>> GetAllAsync() => await _context.Rooms.ToListAsync();

    public async Task<Room?> GetByIdAsync(Guid id) => await _context.Rooms.FindAsync(id);

    public async Task AddAsync(Room room)
    {
        room.Id = Guid.NewGuid();
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }
}
