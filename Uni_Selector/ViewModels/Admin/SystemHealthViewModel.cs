namespace Uni_Selector.ViewModels.Admin
{
    public class SystemHealthViewModel
    {
        // Database Health
        public string DatabaseStatus { get; set; }
        public long TotalDatabaseSize { get; set; }

        // System Metrics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int UsersLoggedInLast24Hours { get; set; }

        // Application Metrics
        public int TotalApplications { get; set; }
        public int ApplicationsLast24Hours { get; set; }
        public int ApplicationsLast7Days { get; set; }
        public int ApplicationsLast30Days { get; set; }

        // Notification Metrics
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int NotificationsSent24Hours { get; set; }
        public int FailedEmailNotifications { get; set; }

        // Commission & Financial Metrics
        public int TotalCommissions { get; set; }
        public int SettledCommissions { get; set; }
        public int PendingCommissions { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal SettledCommissionAmount { get; set; }

        // Discount Metrics
        public int TotalDiscounts { get; set; }
        public int IssuedDiscounts { get; set; }
        public int RedeemedDiscounts { get; set; }
        public int ExpiredDiscounts { get; set; }

        // University & Program Metrics
        public int TotalUniversities { get; set; }
        public int ActiveUniversities { get; set; }
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int PendingBtecApprovals { get; set; }

        // Recommendation Metrics
        public int TotalRecommendations { get; set; }
        public int RecommendationsLast24Hours { get; set; }
        public int ViewedRecommendations { get; set; }

        // Performance Metrics
        public double AverageResponseTime { get; set; }
        public TimeSpan ServerUptime { get; set; }
        public double MemoryUsage { get; set; }
        public double CpuUsage { get; set; }

        // Response Time Details
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public double MedianResponseTime { get; set; }
        public double Last1MinuteResponseTime { get; set; }
        public double Last5MinutesResponseTime { get; set; }
        public int RequestsPerMinute { get; set; }
        public double SuccessRate { get; set; }

        // Error & Log Metrics
        public int ErrorsLast24Hours { get; set; }
        public int WarningsLast24Hours { get; set; }

        // Last Update
        public DateTime LastHealthCheckTime { get; set; }

        // Calculated Properties
        public double UserActivityRate => TotalUsers > 0
            ? (double)UsersLoggedInLast24Hours / TotalUsers * 100
            : 0;

        public double NotificationSuccessRate => TotalNotifications > 0
            ? (double)(TotalNotifications - FailedEmailNotifications) / TotalNotifications * 100
            : 0;

        public double CommissionSettlementRate => TotalCommissions > 0
            ? (double)SettledCommissions / TotalCommissions * 100
            : 0;

        public double DiscountRedemptionRate => TotalDiscounts > 0
            ? (double)RedeemedDiscounts / TotalDiscounts * 100
            : 0;

        public double RecommendationViewRate => TotalRecommendations > 0
            ? (double)ViewedRecommendations / TotalRecommendations * 100
            : 0;

        public string OverallSystemHealth
        {
            get
            {
                if (DatabaseStatus == "Healthy" &&
                    MemoryUsage < 1000 &&
                    FailedEmailNotifications < 100)
                    return "Healthy";
                else if (FailedEmailNotifications > 500 || MemoryUsage > 2000)
                    return "Critical";
                else
                    return "Warning";
            }
        }

        public string DatabaseSizeFormatted => FormatBytes(TotalDatabaseSize);

        public string MemoryUsageFormatted => $"{MemoryUsage:F2} MB";

        public string ServerUptimeFormatted
        {
            get
            {
                if (ServerUptime.TotalDays >= 1)
                    return $"{(int)ServerUptime.TotalDays}d {ServerUptime.Hours}h";
                else if (ServerUptime.TotalHours >= 1)
                    return $"{(int)ServerUptime.TotalHours}h {ServerUptime.Minutes}m";
                else
                    return $"{ServerUptime.Minutes}m {ServerUptime.Seconds}s";
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
