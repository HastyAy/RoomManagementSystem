
namespace RoomManager.RoomService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // config files laden
            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json", optional: true);

       
            builder.AddRoomDataSource();
            builder.AddRoomRepositories();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
