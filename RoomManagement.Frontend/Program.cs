using Radzen;
using RoomManagement.Frontend.Components;
using RoomManager.Frontend.Services;

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
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

            builder.Services.AddRadzenComponents();
            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.AddFrontendServices();

            builder.Services.AddHttpClient<RoomService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7012"); 
            });

            builder.Services.AddHttpClient<StudentService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7084");
            });

            builder.Services.AddHttpClient<ProfessorService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7084");
            });

            builder.Services.AddHttpClient<BookingService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7084");
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
