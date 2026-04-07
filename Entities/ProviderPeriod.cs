namespace InternetMonitor.Api.Entities
{
    public class ProviderPeriod
    {
        public long Id { get; set; }
        
        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;

        public DateTime StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }

        public string? TariffName { get; set; }
        public string? Comment { get; set; }
    }
}
