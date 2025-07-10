using Radzen;
using RoomManagement.Frontend.Components;
using RoomManagement.Frontend.Services;

namespace RoomManagement.Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var environment = builder.Environment.EnvironmentName;

            // Load the appropriate appsettings file
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

            builder.Services.AddRadzenComponents();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.AddFrontendServices();

            // Get service URLs from configuration
            var serviceUrls = builder.Configuration.GetSection("ServiceUrls");

            // Updated to use configuration-based URLs for Docker and localhost
            builder.Services.AddHttpClient<RoomService>(client =>
            {
                var roomServiceUrl = serviceUrls["RoomService"] ?? "https://localhost:7001";
                client.BaseAddress = new Uri(roomServiceUrl);
            });

            builder.Services.AddHttpClient<StudentService>(client =>
            {
                var userServiceUrl = serviceUrls["UserService"] ?? "https://localhost:7002";
                client.BaseAddress = new Uri(userServiceUrl);
            });

            builder.Services.AddHttpClient<ProfessorService>(client =>
            {
                var userServiceUrl = serviceUrls["UserService"] ?? "https://localhost:7002";
                client.BaseAddress = new Uri(userServiceUrl);
            });

            builder.Services.AddHttpClient<BookingService>(client =>
            {
                var bookingServiceUrl = serviceUrls["BookingService"] ?? "https://localhost:7003";
                client.BaseAddress = new Uri(bookingServiceUrl);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Note: In Docker, we don't use HTTPS redirection since we're using HTTP internally
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}