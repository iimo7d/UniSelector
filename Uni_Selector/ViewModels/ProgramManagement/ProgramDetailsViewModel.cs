using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class ProgramDetailsViewModel
    {
        public int Id { get; set; }
        public string ProgramNameArabic { get; set; }
        public string ProgramNameEnglish { get; set; }
        public string? Description { get; set; }
        public Degree Degree { get; set; }
        public LanguageCode Language { get; set; }
        public int TotalCreditHours { get; set; }
        public StudySystem StudySystem { get; set; }
        public int DurationInYears { get; set; }
        public DateTime SemesterStartDate { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal RegistrationFeeRegularSemester { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UniversityNameArabic { get; set; }
        public string UniversityNameEnglish { get; set; }
        public List<EntryRequirementDto> EntryRequirements { get; set; } = new();
    }
}
