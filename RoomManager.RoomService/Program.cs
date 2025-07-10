using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RoomManager.RoomService.Infrastructure;
using RoomManager.RoomService.Infrastructure.Persistence;
using System.Reflection;

namespace RoomManager.RoomService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var env = builder.Environment.EnvironmentName;

            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables();

            // ConnectionString aus Config holen (jetzt auch aus ENV möglich)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Services registrieren - Use fixed server version to avoid early connection
            builder.Services.AddDbContext<RoomDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 42)), // Use fixed version instead of AutoDetect
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    )
                )
            );

            builder.Services.AddRoomService();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Room Management API",
                    Version = "v1",
                    Description = GetApiDescription(builder.Environment.ContentRootPath)
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auto-Generated API");
                    c.SwaggerEndpoint("/api-docs/openapi.yaml", "Manual OpenAPI Spec");
                });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Room Management API");
                    c.RoutePrefix = "swagger";
                });
            }

            app.MapGet("/api-docs/openapi.yaml", () =>
            {
                var specPath = Path.Combine(app.Environment.ContentRootPath, "docs", "openapi.yaml");
                return File.Exists(specPath)
                    ? Results.File(specPath, "application/x-yaml", "openapi.yaml")
                    : Results.NotFound();
            });

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            ApplyDatabaseMigrations(app);
            app.Run();
        }

        private static string GetApiDescription(string contentRootPath)
        {
            try
            {
                var readmePath = Path.Combine(contentRootPath, "docs", "README.md");
                if (File.Exists(readmePath))
                    return File.ReadAllText(readmePath);
            }
            catch { }

            return "API for managing rooms in the Room Management System using Hexagonal Architecture.";
        }

        private static void ApplyDatabaseMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            var retryCount = 0;
            const int maxRetries = 20;
            const int delayMs = 5000;

            while (retryCount < maxRetries)
            {
                try
                {
                    logger.LogInformation($"Attempting to connect to database (attempt {retryCount + 1}/{maxRetries})...");

                    // Test connection first
                    if (dbContext.Database.CanConnect())
                    {
                        logger.LogInformation("Database connection established. Applying migrations...");
                        dbContext.Database.Migrate();
                        logger.LogInformation("Room Service database migrations applied successfully.");
                        return;
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot connect to database");
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    logger.LogWarning($"Migration attempt {retryCount} failed: {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        logger.LogError(ex, "Max retry attempts reached. An error occurred while applying Room Service database migrations.");
                        if (app.Environment.IsDevelopment())
                        {
                            throw;
                        }
                        else
                        {
                            logger.LogCritical("Application starting without database migrations applied!");
                            return;
                        }
                    }

                    logger.LogInformation($"Retrying in {delayMs}ms...");
                    Thread.Sleep(delayMs);
                }
            }
        }
    }
}