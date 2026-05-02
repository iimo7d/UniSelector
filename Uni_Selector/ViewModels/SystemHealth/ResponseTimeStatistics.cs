namespace Uni_Selector.ViewModels.SystemHealth
{
    public class ResponseTimeStatistics
    {
        public long TotalRequests { get; set; }
        public double AverageResponseTime { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public double MedianResponseTime { get; set; }

        // Recent averages
        public double Last100Average { get; set; }
        public double Last1MinuteAverage { get; set; }
        public double Last5MinutesAverage { get; set; }
        public double Last15MinutesAverage { get; set; }
        public double Last1HourAverage { get; set; }

        // Status codes
        public int SuccessfulRequests { get; set; }
        public int ClientErrors { get; set; }
        public int ServerErrors { get; set; }

        // Performance buckets
        public int FastRequests { get; set; }        // < 100ms
        public int NormalRequests { get; set; }      // 100-500ms
        public int SlowRequests { get; set; }        // 500-1000ms
        public int VerySlow { get; set; }           // > 1000ms

        // Request rates
        public int RequestsLastMinute { get; set; }
        public int RequestsLastHour { get; set; }

        // Endpoint analysis
        public List<EndpointPerformance> SlowestEndpoints { get; set; } = new();

        // Calculated properties
        public double SuccessRate => TotalRequests > 0
            ? (double)SuccessfulRequests / TotalRequests * 100
            : 0;

        public double ErrorRate => TotalRequests > 0
            ? (double)(ClientErrors + ServerErrors) / TotalRequests * 100
            : 0;

        public string PerformanceStatus
        {
            get
            {
                if (AverageResponseTime < 100) return "Excellent";
                if (AverageResponseTime < 300) return "Good";
                if (AverageResponseTime < 500) return "Fair";
                if (AverageResponseTime < 1000) return "Poor";
                return "Critical";
            }
        }
    }
}
