namespace InternetMonitor.Api.Options
{
    public class InternetMonitorOptions
    {
        public const string SectionName = "InternetMonitor";

        public int IntervalSeconds { get; set; } = 60;
        public int TimeoutSeconds { get; set; } = 10;
        public string TargetUrl { get; set; } = "https://www.google.com/generate_204";
    }
}
