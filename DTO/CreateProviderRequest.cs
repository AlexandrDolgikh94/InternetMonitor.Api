namespace InternetMonitor.Api.DTO
{
    public class CreateProviderRequest
    {
        public string Name { get; set; } = null!;
        public string? Comment { get; set; }
    }
}
