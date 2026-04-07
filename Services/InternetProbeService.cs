using InternetMonitor.Api.Data;
using InternetMonitor.Api.Entities;
using InternetMonitor.Api.Options;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace InternetMonitor.Api.Services
{
    public class InternetProbeService : IInternetProbeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _dbContext;
        private readonly IProviderService _providerService;
        private readonly InternetMonitorOptions _options;
        private readonly ILogger<InternetProbeService> _logger;

        public InternetProbeService(
            IHttpClientFactory httpClientFactory,
            AppDbContext dbContext,
            IProviderService providerService,
            IOptions<InternetMonitorOptions> options,
            ILogger<InternetProbeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
            _providerService = providerService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task ExecuteCheckAsync(CancellationToken cancelationToken)
        {
            var providerId = await _providerService.GetActiveProviderIdAscync(cancelationToken);

            if (!providerId.HasValue)
            {
                _logger.LogWarning("Internet check skipped because there is no active provider.");
                return;
            }

            var client = _httpClientFactory.CreateClient("internet-monitor");

            using var request = new HttpRequestMessage(HttpMethod.Get, _options.TargetUrl);

            var startedAt = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            bool isSuccess = false;
            int? statusCode = null;
            string? errorMessage = null;

            try
            {
                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancelationToken);

                statusCode = (int)response.StatusCode;

                isSuccess = response.IsSuccessStatusCode;
            }
            catch (OperationCanceledException) when (!cancelationToken.IsCancellationRequested)
            {
                errorMessage = "Request timed out.";
            }
            catch (HttpRequestException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
            }

            var check = new InternetCheck
            {
                CheckedAtUtc = startedAt,
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                DurationMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = errorMessage,
                TargetUrl = _options.TargetUrl,
                ProviderId = providerId.Value
            };

            _dbContext.InternetChecks.Add(check);
            await _dbContext.SaveChangesAsync(cancelationToken);
        }
    }
}
