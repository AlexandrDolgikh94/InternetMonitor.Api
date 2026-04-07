namespace InternetMonitor.Api.Entities
{
    public class InternetCheck
    {
        public long Id { get; set; }

        public DateTime CheckedAtUtc { get; set; }

        public bool IsSuccess { get; set; }
        public int? StatusCode { get; set; }
        public long DurationMs { get; set; }
        public string? ErrorMessage { get; set; }

        public string TargetUrl { get; set; } = null!;

        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;
    }
}
