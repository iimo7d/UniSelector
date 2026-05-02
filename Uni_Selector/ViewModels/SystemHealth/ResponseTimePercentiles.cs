namespace Uni_Selector.ViewModels.SystemHealth
{
    public class ResponseTimePercentiles
    {
        public double P50 { get; set; }  // Median
        public double P75 { get; set; }
        public double P90 { get; set; }
        public double P95 { get; set; }
        public double P99 { get; set; }

        public string P50Formatted => $"{P50:F2} ms";
        public string P95Formatted => $"{P95:F2} ms";
        public string P99Formatted => $"{P99:F2} ms";
    }
}
