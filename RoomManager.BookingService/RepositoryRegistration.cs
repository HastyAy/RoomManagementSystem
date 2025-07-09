using RoomManager.BookingService.Repositories;

namespace RoomManager.BookingService;

public static class RepositoryRegistration
{
    public static IHostApplicationBuilder AddBookingRepositories(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        return builder;
    }
}
