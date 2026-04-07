namespace InternetMonitor.Api.Entities
{
    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Comment { get; set; }

        public ICollection<ProviderPeriod> Periods { get; set; } = new List<ProviderPeriod>();
        public ICollection<InternetCheck> Checks { get; set; } = new List<InternetCheck>();


    }
}
