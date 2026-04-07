using InternetMonitor.Api.Data;
using InternetMonitor.Api.DTO;
using InternetMonitor.Api.Entities;
using InternetMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/providers")]
    public class ProviderController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IProviderService _providerService;

        public ProviderController(
            AppDbContext dbContext,
            IProviderService providerService)
        {
            _dbContext = dbContext;
            _providerService = providerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProviders(CancellationToken cancellationToken)
        {
            var providers = await _dbContext.Providers
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Comment
                })
                .ToListAsync(cancellationToken);

            return Ok(providers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProvider(
            [FromBody] CreateProviderRequest request,
            CancellationToken cancellationToken)
        {
            var provider = new Provider
            {
                Name = request.Name,
                Comment = request.Comment
            };

            _dbContext.Providers.Add(provider);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                provider.Id,
                provider.Name,
                provider.Comment
            });
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> ActivateProvider(
            int id,
            [FromBody] ActivateProviderRequest request,
            CancellationToken cancelationToken)
        {
            await _providerService.ActivateProviderAsync(
                id,
                request.TariffName,
                request.Comment,
                cancelationToken);

            return Ok(new { Message = "Provider activated" });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveProvider(CancellationToken cancellationToken)
        {
            var activePeriod = await _dbContext.ProviderPeriods
                .AsNoTracking()
                .Include(pp => pp.Provider)
                .OrderByDescending(pp => pp.StartUtc)
                .FirstOrDefaultAsync(pp => pp.EndUtc == null, cancellationToken);

            if (activePeriod == null)
                return Ok(null);

            return Ok(new
            {
                activePeriod.Provider.Id,
                ProviderName = activePeriod.Provider.Name,
                activePeriod.StartUtc,
                activePeriod.TariffName,
                activePeriod.Comment
            });
        }
    }
}
