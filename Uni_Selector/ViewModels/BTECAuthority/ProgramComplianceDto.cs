using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class ProgramComplianceDto
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public string University { get; set; }
        public string UniversityCity { get; set; }
        public BtecLevel Level { get; set; }
        public string TechnicalField { get; set; }
        public ComplianceStatus ComplianceStatus { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public string? ComplianceNotes { get; set; }
    }
}
