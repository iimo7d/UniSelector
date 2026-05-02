namespace Uni_Selector.ViewModels.UniversityReps
{
    public class ProgramAnalyticsViewModel
    {
        public List<ProgramPerformanceViewModel> Programs { get; set; } = new();
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int TotalApplications { get; set; }
        public decimal AverageConversionRate { get; set; }
        public Dictionary<string, int> DegreeDistribution { get; set; } = new();
        public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    }
}
