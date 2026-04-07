using InternetMonitor.Api.Data;
using InternetMonitor.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public DashboardController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var from = now.AddHours(-24);

            var lastCheck = await _dbContext.InternetChecks
                .AsNoTracking()
                .Include(x => x.Provider)
                .OrderByDescending(x => x.CheckedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            var checks24h = await _dbContext.InternetChecks
                .AsNoTracking()
                .Where(x => x.CheckedAtUtc >= from)
                .ToListAsync(cancellationToken);

            var totalChecks = checks24h.Count;
            var successChecks = checks24h.Count(x => x.IsSuccess);
            var failedChecks = totalChecks - successChecks;

            var successfulDurations = checks24h
                .Where(x => x.IsSuccess)
                .Select(x => (double)x.DurationMs)
                .ToList();

            var dto = new DashboardSummaryDto
            {
                CurrentProviderName = lastCheck?.Provider.Name,
                LastCheckSuccess = lastCheck?.IsSuccess,
                LastCheckedAtUtc = lastCheck?.CheckedAtUtc,
                LastDurationMs = lastCheck?.DurationMs,
                TotalChecks24h = totalChecks,
                SuccessChecks24h = successChecks,
                FailedChecks24h = failedChecks,
                UptimePercentage24h = totalChecks == 0 ? 0 : (double)successChecks / totalChecks * 100.0,
                AverageLatencyMs24h = successfulDurations.Count == 0 ? null : successfulDurations.Average()
            };

            return Ok(dto);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int hours = 24, CancellationToken cancellationToken = default)
        {
            if (hours <= 0)
                hours = 24;

            var from = DateTime.UtcNow.AddHours(-hours);

            var history = await _dbContext.InternetChecks
                .AsNoTracking()
                .Include(x => x.Provider)
                .Where(x => x.CheckedAtUtc >= from)
                .OrderBy(x => x.CheckedAtUtc)
                .Select(x => new
                {
                    x.Id,
                    x.CheckedAtUtc,
                    x.IsSuccess,
                    x.StatusCode,
                    x.DurationMs,
                    x.ErrorMessage,
                    x.TargetUrl,
                    x.ProviderId,
                    ProviderName = x.Provider.Name
                })
                .ToListAsync(cancellationToken);

            return Ok(history);
        }

        [HttpGet("providers/compare")]
        public async Task<IActionResult> CompareProviders(CancellationToken cancellationToken)
        {
            var compare = await _dbContext.InternetChecks
                .AsNoTracking()
                .GroupBy(x => x.ProviderId)
                .Select(g => new
                {
                    ProviderId = g.Key,
                    TotalChecks = g.Count(),
                    SuccessChecks = g.Count(x => x.IsSuccess),
                    FailedChecks = g.Count(x => !x.IsSuccess),
                    AverageLatencyMs = g.Where(x => x.IsSuccess).Select(x => (double?)x.DurationMs).Average()
                })
                .Join(
                    _dbContext.Providers.AsNoTracking(),
                    x => x.ProviderId,
                    p => p.Id,
                    (x, p) => new
                    {
                        x.ProviderId,
                        ProviderName = p.Name,
                        x.TotalChecks,
                        x.SuccessChecks,
                        x.FailedChecks,
                        UptimePercent = x.TotalChecks == 0 ? 0 : (double)x.SuccessChecks / x.TotalChecks * 100.0,
                        x.AverageLatencyMs
                    })
                .OrderByDescending(x => x.UptimePercent)
                .ToListAsync(cancellationToken);

            return Ok(compare);
        }
    }
}
