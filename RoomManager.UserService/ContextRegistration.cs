using Microsoft.EntityFrameworkCore;
using RoomManager.UserService.Context;

namespace RoomManager.UserService
{
    public static class ContextRegistration
    {
        public static IHostApplicationBuilder AddUserDataSource(this IHostApplicationBuilder builder)
        {
            var conn = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseMySql(conn, ServerVersion.Parse("10.5")));

            return builder;
        }
    }
}
