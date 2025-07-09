using RoomManager.UserService.Repositories;

namespace RoomManager.UserService
{
    public static class RepositoryRegistration
    {
        public static IHostApplicationBuilder AddUserRepositories(this IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IProfessorRepository, ProfessorRepository>();
            return builder;
        }
    }
}
