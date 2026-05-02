using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Home
{
    public class UniversityProgramCardViewModel
    {
        public int Id { get; set; }
        public int UniversityId { get; set; }
        public string ProgramNameArabic { get; set; }
        public string ProgramNameEnglish { get; set; }
        public Degree Degree { get; set; }
        public LanguageCode Language { get; set; }
        public StudySystem StudySystem { get; set; }
        public int DurationInYears { get; set; }
        public int TotalCreditHours { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public DateTime SemesterStartDate { get; set; }

        // Entry Requirements Summary
        public double MinGPA { get; set; }
        public List<string> AcceptedPaths { get; set; } = new();
    }
}
