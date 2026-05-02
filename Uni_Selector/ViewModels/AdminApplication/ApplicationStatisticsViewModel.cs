namespace Uni_Selector.ViewModels.AdminApplication
{
    public class ApplicationStatisticsViewModel
    {
        // Summary Cards
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int EnrolledApplications { get; set; }

        // Conversion Metrics
        public decimal ApprovalRate { get; set; }
        public decimal EnrollmentRate { get; set; }
        public decimal RejectionRate { get; set; }

        // Status Distribution (for pie chart)
        public List<StatusDistribution> StatusDistribution { get; set; } = new();

        // Monthly Applications (for line chart - last 12 months)
        public List<MonthlyApplicationCount> MonthlyApplications { get; set; } = new();

        // Top Universities (for column chart)
        public List<UniversityApplicationCount> TopUniversities { get; set; } = new();

        // Top Programs
        public List<ProgramApplicationCount> TopPrograms { get; set; } = new();

        // Application Type Distribution
        public int RegularApplications { get; set; }
        public int BtecApplications { get; set; }

        // Recent Activity
        public List<RecentApplicationActivity> RecentActivity { get; set; } = new();

        // Average Processing Time
        public double AverageProcessingDays { get; set; }
    }
}
