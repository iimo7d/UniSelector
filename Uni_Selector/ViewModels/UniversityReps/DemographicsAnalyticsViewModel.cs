namespace Uni_Selector.ViewModels.UniversityReps
{
    public class DemographicsAnalyticsViewModel
    {
        public int TotalStudents { get; set; }
        public double AverageGPA { get; set; }
        public Dictionary<string, int> GenderDistribution { get; set; } = new();
        public Dictionary<string, int> PathDistribution { get; set; } = new();
        public Dictionary<string, int> AcademicTrackDistribution { get; set; } = new();
        public List<ProvinceStatViewModel> ProvinceDistribution { get; set; } = new();
        public Dictionary<string, int> GPARangeDistribution { get; set; } = new();
        public Dictionary<string, double> AverageGPAByPath { get; set; } = new();
    }
}
