namespace Uni_Selector.ViewModels.Admin
{
    public class UniversityStatsDto
    {
        public string UniversityName { get; set; }
        public int TotalPrograms { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }

        public decimal ApprovalRate => TotalApplications > 0
            ? (decimal)ApprovedApplications / TotalApplications * 100
            : 0;
    }

}
