using InternetMonitor.Api.Data;
using InternetMonitor.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternetMonitor.Api.Services
{
    public class ProviderService : IProviderService
    {
        private readonly AppDbContext _dbContext;

        public ProviderService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int?> GetActiveProviderIdAscync(CancellationToken cancellationToken)
        {
            var activePeriod = await _dbContext.ProviderPeriods
                .AsNoTracking()
                .OrderByDescending(pp => pp.StartUtc)
                .FirstOrDefaultAsync(pp => pp.EndUtc == null, cancellationToken);

            return activePeriod?.ProviderId;
        }

        public async Task ActivateProviderAsync(
            int providerId,
            string? tariffName,
            string? comment,
            CancellationToken cancellationToken)
        {
            var providerExists = await _dbContext.Providers.AnyAsync(p => p.Id == providerId, cancellationToken);

            if (!providerExists)
            {
                throw new ArgumentException($"Provider with id {providerId} not found.");
            }

            var now = DateTime.UtcNow;

            var activePeriod = await _dbContext.ProviderPeriods
                .Where(pp => pp.EndUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var period in activePeriod)
            {
                period.EndUtc = now;
            }

            var newPeriod = new ProviderPeriod
            {
                ProviderId = providerId,
                StartUtc = now,
                EndUtc = null,
                TariffName = tariffName,
                Comment = comment
            };

            _dbContext.ProviderPeriods.Add(newPeriod);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
