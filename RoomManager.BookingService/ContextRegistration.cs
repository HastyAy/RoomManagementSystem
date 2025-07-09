using Microsoft.EntityFrameworkCore;
using RoomManager.BookingService.Context;
namespace RoomManager.BookingService;

public static class ContextRegistration
{
    public static IHostApplicationBuilder AddBookingDataSource(this IHostApplicationBuilder builder)
    {
        var conn = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<BookingDbContext>(options =>
            options.UseMySql(conn, ServerVersion.Parse("10.5")));

        return builder;
    }
}
