using InternetMonitor.Api.Options;
using InternetMonitor.Api.Services;
using Microsoft.Extensions.Options;

namespace InternetMonitor.Api.Background
{
    public class InternetMonitoringBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InternetMonitoringBackgroundService> _logger;
        private readonly InternetMonitorOptions _options;

        public InternetMonitoringBackgroundService(
            IServiceProvider serviceProvider,
            IOptions<InternetMonitorOptions> options,
            ILogger<InternetMonitoringBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Internet monitoring background service is starting.");

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalSeconds));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var probeService = scope.ServiceProvider.GetRequiredService<IInternetProbeService>();

                    await probeService.ExecuteCheckAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the internet probe.");
                }

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);

                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Internet monitoring background service is stopping.");
        }
    }
}
