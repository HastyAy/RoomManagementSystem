using Microsoft.EntityFrameworkCore;
using RoomManager.UserService.Context; // Adjust this to your actual DbContext namespace

namespace RoomManager.UserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Config files
            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json", optional: true).AddEnvironmentVariables();

            // Add services
            builder.AddUserDataSource();
            builder.AddUserRepositories();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS for frontend
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Add logging
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            var app = builder.Build();

            // Configure pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                // Still enable Swagger in production for Docker (optional)
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();

            // Apply EF migrations BEFORE app.Run()
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>(); // Adjust to your actual DbContext class name
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Applying User Service database migrations...");
                    dbContext.Database.Migrate();
                    logger.LogInformation("User Service database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying User Service database migrations.");
                    throw; // Re-throw to prevent the app from starting with a broken database
                }
            }

            app.Run();
        }
    }
}