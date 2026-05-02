namespace Uni_Selector.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Total Counts
        public int TotalUsers { get; set; }
        public int TotalUniversities { get; set; }
        public int TotalPrograms { get; set; }
        public int TotalApplications { get; set; }

        // Active Counts
        public int ActiveUniversities { get; set; }
        public int ActivePrograms { get; set; }

        // Application Statistics
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int EnrolledApplications { get; set; }

        // Financial Statistics
        public decimal TotalCommissionsGenerated { get; set; }
        public decimal PendingCommissions { get; set; }
        public int TotalDiscountsIssued { get; set; }
        public int TotalDiscountsRedeemed { get; set; }

        // Recent Activity (Last 30 days)
        public int NewUsersLast30Days { get; set; }
        public int NewApplicationsLast30Days { get; set; }
        public int NewUniversitiesLast30Days { get; set; }

        // Collections
        public List<ApplicationSummaryDto> RecentApplications { get; set; } = new();
        public List<UserSummaryDto> RecentUsers { get; set; } = new();
        public List<UniversityStatsDto> UniversityStats { get; set; } = new();
        public List<StatusCountDto> ApplicationsByStatus { get; set; } = new();
        public List<MonthlyStatDto> MonthlyApplications { get; set; } = new();

        // System Health Indicators
        public int UnreadNotifications { get; set; }
        public int PendingBtecApprovals { get; set; }

        // Calculated Properties
        public decimal ApprovalRate => TotalApplications > 0
            ? (decimal)ApprovedApplications / TotalApplications * 100
            : 0;

        public decimal DiscountRedemptionRate => TotalDiscountsIssued > 0
            ? (decimal)TotalDiscountsRedeemed / TotalDiscountsIssued * 100
            : 0;

        public string UserGrowthTrend => NewUsersLast30Days > 0 ? "up" : "stable";
        public string ApplicationGrowthTrend => NewApplicationsLast30Days > 0 ? "up" : "stable";
    }
}
