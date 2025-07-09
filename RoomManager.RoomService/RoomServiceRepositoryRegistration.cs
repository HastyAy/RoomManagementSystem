using Microsoft.Extensions.DependencyInjection;
using RoomManager.RoomService.Repositories;

namespace RoomManager.RoomService;

public static class RoomServiceRepositoryRegistration
{
    public static IHostApplicationBuilder AddRoomRepositories(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRoomRepository, RoomRepository>();
        return builder;
    }
}
