using Microsoft.EntityFrameworkCore;
using RoomManager.RoomService.Application.Ports;
using RoomManager.RoomService.Infrastructure.Persistence;

namespace RoomManager.RoomService.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRoomService(this IServiceCollection services)
        {
            // Application Services
            services.AddScoped<IRoomService, Application.Services.RoomService>();

            // Infrastructure
            services.AddScoped<IRoomRepository, EfRoomRepository>();

            return services;
        }

        public static IServiceCollection AddRoomDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<RoomDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.Parse("10.5")));

            return services;
        }
    }
}