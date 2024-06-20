using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
namespace WebScraping;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
     
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlite(connectionString);
        });
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7081") }); // Replace with your ASP.NET Core API URL

        builder.Services.AddScoped<WebScraperService>();
        builder.Services.AddScoped<DatabaseService>();

        await builder.Build().RunAsync();
    }
}
