namespace InternetMonitor.Api.DTO
{
    public class DashboardSummaryDto
    {
        public string? CurrentProviderName { get; set; }
        public bool? LastCheckSuccess { get; set; }
        public DateTime? LastCheckedAtUtc { get; set; }
        public long? LastDurationMs { get; set; }

        public int TotalChecks24h { get; set; }
        public int SuccessChecks24h { get; set; }
        public int FailedChecks24h { get; set; }
        public double UptimePercentage24h { get; set; }
        public double? AverageLatencyMs24h { get; set; }
    }
}
