namespace RoomManager.BookingService
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
                    .AddJsonFile($"appsettings.{env}.json", optional: true);

                // Add services
                builder.AddBookingDataSource();
                builder.AddBookingRepositories();
                builder.AddBookingServices();

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

                app.UseCors();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
        }
}
