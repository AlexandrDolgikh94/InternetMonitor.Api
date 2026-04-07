namespace InternetMonitor.Api.Services
{
    public interface IInternetProbeService
    {
        Task ExecuteCheckAsync(CancellationToken cancellationToken);
    }
}
