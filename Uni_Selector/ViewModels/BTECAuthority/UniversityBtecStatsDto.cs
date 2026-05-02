namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class UniversityBtecStatsDto
    {
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public string City { get; set; }
        public int TotalPrograms { get; set; }
        public int ApprovedPrograms { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public double ComplianceRate { get; set; }
    }

}
