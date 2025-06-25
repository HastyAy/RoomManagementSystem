using RoomManager.Shared.Repositories;
using RoomManager.BookingService.Repositories;

namespace RoomManager.BookingService;

public static class RepositoryRegistration
{
    public static IHostApplicationBuilder AddBookingRepositories(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        builder.Services.AddScoped<IProfessorRepository, ProfessorRepository>();
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        return builder;
    }
}
