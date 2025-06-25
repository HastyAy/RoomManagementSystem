using Microsoft.EntityFrameworkCore;
using RoomManager.Shared.Entities;

namespace RoomManager.RoomService.Context;

public class RoomDbContext : DbContext
{
    public RoomDbContext(DbContextOptions<RoomDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
}
