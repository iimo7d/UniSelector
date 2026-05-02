namespace Uni_Selector.ViewModels.UniversityReps
{
    public class UniversityRepDashboardViewModel
    {
        public string UniversityName { get; set; }
        public string RepresentativeName { get; set; }

        // KPI Cards
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int ApplicationsThisMonth { get; set; }
        public int ApplicationsLastMonth { get; set; }
        public int ActivePrograms { get; set; }
        public int ActiveBtecPrograms { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public decimal PendingSettlement { get; set; }

        // Charts Data
        public Dictionary<string, int> ApplicationStatusBreakdown { get; set; } = new();
        public List<MonthlyStatViewModel> MonthlyApplicationsTrend { get; set; } = new();

        // Recent Activity
        public List<RecentApplicationViewModel> RecentApplications { get; set; } = new();
    }
}
