using Microsoft.EntityFrameworkCore;
using RoomManager.RoomService.Infrastructure.Persistence;
using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using RoomManager.RoomService.Infrastructure;

namespace RoomManager.RoomService
{
    public class Program
    {
        // Custom metrics
        private static readonly Meter Meter = new("RoomService");
        private static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>("http_requests_total");

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var env = builder.Environment.EnvironmentName;

            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add OpenTelemetry - Minimale Version
            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddMeter("RoomService")
                        .AddAspNetCoreInstrumentation()
                        .AddPrometheusExporter();
                });

            // Services registrieren
            builder.Services.AddDbContext<RoomDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(9, 3, 0)),
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

            // Simple health checks
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

            // Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Room Management API",
                    Version = "v1"
                });
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

            // Configure pipeline
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Room Management API");
                c.RoutePrefix = "swagger";
            });

            // Add Prometheus metrics endpoint
            app.MapPrometheusScrapingEndpoint();

            // Add health check endpoints
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/ready");

            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();

            ApplyDatabaseMigrations(app);
            app.Run();
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
                        logger.LogError(ex, "Max retry attempts reached.");
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