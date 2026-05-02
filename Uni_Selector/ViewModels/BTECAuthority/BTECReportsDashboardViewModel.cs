namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BTECReportsDashboardViewModel
    {
        // Summary Statistics
        public int TotalBtecPrograms { get; set; }
        public int ActiveBtecPrograms { get; set; }
        public int TotalBtecStudents { get; set; }
        public int TotalBtecApplications { get; set; }
        public int UniversitiesOfferingBtec { get; set; }

        // Trends
        public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
        public List<ProgramComplianceDto> ComplianceByLevel { get; set; } = new();
        public List<UniversityBtecStatsDto> TopUniversities { get; set; } = new();
        public List<FieldStatisticsDto> ProgramsByField { get; set; } = new();
    }

}
