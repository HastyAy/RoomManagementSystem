using RoomManager.BookingService.Services;

namespace RoomManager.BookingService
{
    public static class ServiceRegistration
    {
        public static IHostApplicationBuilder AddBookingServices(this IHostApplicationBuilder builder)
        {
            // Register HttpClients for inter-service communication
            var roomServiceUrl = builder.Configuration["ServiceUrls:RoomService"] ?? "https://localhost:7001";
            var userServiceUrl = builder.Configuration["ServiceUrls:UserService"] ?? "https://localhost:7002";

            builder.Services.AddHttpClient("RoomService", client =>
            {
                client.BaseAddress = new Uri(roomServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddHttpClient("UserService", client =>
            {
                client.BaseAddress = new Uri(userServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register service clients
            builder.Services.AddScoped<IRoomServiceClient, RoomServiceClient>();
            builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

            // Register business service
            builder.Services.AddScoped<IBookingBusinessService, BookingBusinessService>();

            return builder;
        }
    }
}
