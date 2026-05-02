using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class BtecProgramDetailsViewModel
    {
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
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal RegistrationFeeRegularSemester { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public bool IsApprovedByBtecAuthority { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? ApprovalNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UniversityNameArabic { get; set; }
        public string UniversityNameEnglish { get; set; }
        public List<BtecEntryRequirementDto> EntryRequirements { get; set; } = new();
    }
}
