using Microsoft.EntityFrameworkCore;
using RoomManager.RoomService.Context;

namespace RoomManager.RoomService;

public static class ContextRegistration
{
    public static IHostApplicationBuilder AddRoomDataSource(this IHostApplicationBuilder builder)
    {
        var conn = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<RoomDbContext>(options =>
            options.UseMySql(conn, ServerVersion.Parse("10.5")));

        return builder;
    }
}
