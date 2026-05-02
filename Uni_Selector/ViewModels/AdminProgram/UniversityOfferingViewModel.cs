namespace Uni_Selector.ViewModels.AdminProgram
{
    public class UniversityOfferingViewModel
    {
        public int UniversityId { get; set; }
        public string UniversityNameEnglish { get; set; }
        public string UniversityNameArabic { get; set; }
        public string? UniversityLogoPath { get; set; }
        public string StudySystem { get; set; }
        public int DurationInYears { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public bool IsActive { get; set; }
    }
}
