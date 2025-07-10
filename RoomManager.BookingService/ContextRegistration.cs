using Microsoft.EntityFrameworkCore;
using RoomManager.BookingService.Context;

namespace RoomManager.BookingService;

public static class ContextRegistration
{
    public static IHostApplicationBuilder AddBookingDataSource(this IHostApplicationBuilder builder)
    {
        var conn = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<BookingDbContext>(options =>
            options.UseMySql(
                conn,
                new MySqlServerVersion(new Version(8, 0, 42)), 
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                )
            )
        );

        return builder;
    }
}