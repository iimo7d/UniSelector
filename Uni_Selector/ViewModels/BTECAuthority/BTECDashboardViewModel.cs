namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BTECDashboardViewModel
    {
        // Summary Statistics
        public int TotalBtecPrograms { get; set; }
        public int PendingApproval { get; set; }
        public int ApprovedPrograms { get; set; }
        public int RejectedPrograms { get; set; }

        // University Statistics
        public int TotalUniversities { get; set; }
        public int UniversitiesWithBtec { get; set; }

        // Student Statistics
        public int TotalBtecApplications { get; set; }
        public int ActiveStudents { get; set; }

        // Charts Data
        public List<ProgramsByLevelDto> ProgramsByLevel { get; set; } = new();
        public List<ProgramsByFieldDto> ProgramsByField { get; set; } = new();
        public List<MonthlyApprovalDto> MonthlyApprovals { get; set; } = new();
        public List<UniversityProgramCountDto> TopUniversities { get; set; } = new();

        // Recent Activities
        public List<RecentBtecProgramDto> RecentPrograms { get; set; } = new();
    }

}
