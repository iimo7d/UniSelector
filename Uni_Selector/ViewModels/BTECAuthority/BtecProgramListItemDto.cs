using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BtecProgramListItemDto
    {
        public int Id { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
        public BtecLevel Level { get; set; }
        public string TechnicalField { get; set; }
        public LanguageCode Language { get; set; }

        // University
        public int UniversityId { get; set; }
        public string UniversityNameEnglish { get; set; }
        public string UniversityNameArabic { get; set; }
        public string UniversityCity { get; set; }

        // Program Details
        public int DurationInYears { get; set; }
        public int TotalCreditHours { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public int Capacity { get; set; }

        // Approval Status
        public bool IsActive { get; set; }
        public bool IsApprovedByBtecAuthority { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? ApprovalNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
