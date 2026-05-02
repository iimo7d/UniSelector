using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class RecentBtecProgramDto
    {
        public int Id { get; set; }
        public string NameEnglish { get; set; }
        public string UniversityName { get; set; }
        public BtecLevel Level { get; set; }
        public string TechnicalField { get; set; }
        public bool IsApprovedByBtecAuthority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}
