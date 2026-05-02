namespace Uni_Selector.ViewModels.UniversityReps
{
    public class ApplicationAnalyticsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalApplications { get; set; }
        public double AverageProcessingTimeDays { get; set; }
        public List<StatusDistributionViewModel> StatusDistribution { get; set; } = new();
        public List<DailyStatViewModel> DailyApplications { get; set; } = new();
        public List<ProgramStatViewModel> ProgramStatistics { get; set; } = new();
    }
}
