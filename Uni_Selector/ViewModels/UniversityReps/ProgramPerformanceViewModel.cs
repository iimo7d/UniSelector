namespace Uni_Selector.ViewModels.UniversityReps
{
    public class ProgramPerformanceViewModel
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public string ProgramType { get; set; }
        public string Degree { get; set; }
        public string Language { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int PendingApplications { get; set; }
        public int RecommendationCount { get; set; }
        public decimal ConversionRate { get; set; }
        public int Capacity { get; set; }
        public decimal CapacityUtilization { get; set; }
        public bool IsActive { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFee { get; set; }
    }
}
