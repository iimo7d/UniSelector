namespace Uni_Selector.ViewModels.SystemHealth
{
    public class EndpointPerformance
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public double AverageResponseTime { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public int RequestCount { get; set; }

        public string FullEndpoint => $"{Method} {Path}";
        public string FormattedAverage => $"{AverageResponseTime:F2} ms";
    }
}
