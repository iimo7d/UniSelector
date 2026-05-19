using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BtecProgramDetailsViewModel
    {
        // Program Information
        public int Id { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
        public string? Description { get; set; }
        public BtecLevel Level { get; set; }
        public string TechnicalField { get; set; }
        public LanguageCode Language { get; set; }
        public int DurationInYears { get; set; }
        public DateTime SemesterStartDate { get; set; }
        public int TotalCreditHours { get; set; }

        // Fees
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal RegistrationFeeRegularSemester { get; set; }
        public decimal EstimatedTotalCost { get; set; }

        // Capacity
        public int Capacity { get; set; }

        // University Information
        public int UniversityId { get; set; }
        public string UniversityNameArabic { get; set; }
        public string UniversityNameEnglish { get; set; }
        public string UniversityCity { get; set; }
        public string UniversityProvince { get; set; }
        public string UniversityPhone { get; set; }
        public string UniversityEmail { get; set; }
        public string? UniversityWebsite { get; set; }

        // Approval Status
        public bool IsActive { get; set; }
        public bool IsApprovedByBtecAuthority { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? ApprovalNotes { get; set; }
        public string? RejectionReason { get; set; }

        // Entry Requirements
        public List<BtecEntryRequirementDto> EntryRequirements { get; set; } = new();

        // Statistics
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int EnrolledStudents { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
