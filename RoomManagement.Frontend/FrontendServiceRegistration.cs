using RoomManager.Frontend.Services;

namespace RoomManagement.Frontend;

public static class FrontendServiceRegistration
{
    public static IHostApplicationBuilder AddFrontendServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<RoomService>();
        builder.Services.AddScoped<BookingService>();
        builder.Services.AddScoped<StudentService>();
        builder.Services.AddScoped<ProfessorService>();
        return builder;
    }
}
