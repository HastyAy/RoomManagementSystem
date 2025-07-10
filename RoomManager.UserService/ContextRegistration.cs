using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoomManager.UserService.Context;

namespace RoomManager.UserService
{
    public static class ContextRegistration
    {
        public static IHostApplicationBuilder AddUserDataSource(this IHostApplicationBuilder builder)
        {
            var conn = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseMySql(
                    conn,
                    new MySqlServerVersion(new Version(9, 3, 0)), 
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
}