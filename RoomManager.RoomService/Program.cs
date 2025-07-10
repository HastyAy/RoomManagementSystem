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

            // Configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add services
            builder.Services.AddRoomDatabase(connectionString!);
            builder.Services.AddRoomService();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Swagger/OpenAPI Configuration
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Room Management API",
                    Version = "v1",
                    Description = GetApiDescription(builder.Environment.ContentRootPath)
                });

                // Include XML comments for documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure pipeline - RICHTIGE REIHENFOLGE!
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
                // Optional: Swagger auch in Production (für Docker)
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Room Management API");
                    c.RoutePrefix = "swagger"; // Swagger UI unter /swagger
                });
            }

            // Custom endpoint für statische OpenAPI Spec - VOR UseRouting!
            app.MapGet("/api-docs/openapi.yaml", () =>
            {
                var specPath = Path.Combine(app.Environment.ContentRootPath, "docs", "openapi.yaml");
                return File.Exists(specPath)
                    ? Results.File(specPath, "application/x-yaml", "openapi.yaml")
                    : Results.NotFound();
            });

            // Middleware Pipeline - KORREKTE REIHENFOLGE
            app.UseCors(); // CORS zuerst
            app.UseAuthentication(); // Falls später Authentication hinzugefügt wird
            app.UseAuthorization();

            // Controllers mapping
            app.MapControllers();

            // Database migration - NACH app.Build(), VOR app.Run()
            ApplyDatabaseMigrations(app);

            app.Run();
        }

        // Helper method für API Description
        private static string GetApiDescription(string contentRootPath)
        {
            try
            {
                var readmePath = Path.Combine(contentRootPath, "docs", "README.md");
                if (File.Exists(readmePath))
                {
                    return File.ReadAllText(readmePath);
                }
            }
            catch (Exception)
            {
                // Fallback description falls README nicht gefunden
            }

            return "API for managing rooms in the Room Management System using Hexagonal Architecture.";
        }

        // Separate method für Database Migrations
        private static void ApplyDatabaseMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Applying Room Service database migrations...");
                dbContext.Database.Migrate();
                logger.LogInformation("Room Service database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying Room Service database migrations.");

                // In Development: Exception werfen
                // In Production: Möglicherweise graceful degradation
                if (app.Environment.IsDevelopment())
                {
                    throw;
                }
                else
                {
                    logger.LogCritical("Application starting without database migrations applied!");
                    // Optional: Hier könnten Sie entscheiden, ob die App trotzdem starten soll
                }
            }
        }
    }
}