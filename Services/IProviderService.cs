namespace InternetMonitor.Api.Services
{
    public interface IProviderService
    {
        Task<int?> GetActiveProviderIdAscync(CancellationToken cancellationToken);
        Task ActivateProviderAsync(int providerId, string? tariffName, string? comment, CancellationToken cancellationToken);
    }
}
