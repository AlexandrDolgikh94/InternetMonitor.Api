using InternetMonitor.Api.Background;
using InternetMonitor.Api.Data;
using InternetMonitor.Api.Options;
using InternetMonitor.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseWindowsService();

        builder.WebHost.UseUrls("http://localhost:5000");

        builder.Services.Configure<InternetMonitorOptions>(
            builder.Configuration.GetSection(InternetMonitorOptions.SectionName));

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddHttpClient("internet-monitor", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<InternetMonitorOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("InternetMonitor/1.0");
        });

        builder.Services.AddScoped<IProviderService, ProviderService>();
        builder.Services.AddScoped<IInternetProbeService, InternetProbeService>();

        builder.Services.AddHostedService<InternetMonitoringBackgroundService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("frontend", policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("frontend");

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        app.Run();
    }
}