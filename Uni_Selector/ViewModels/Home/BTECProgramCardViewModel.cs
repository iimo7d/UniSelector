using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Home
{
    public class BTECProgramCardViewModel
    {
        public int Id { get; set; }
        public int UniversityId { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
        public BtecLevel Level { get; set; }
        public string TechnicalField { get; set; }
        public LanguageCode Language { get; set; }
        public int DurationInYears { get; set; }
        public int TotalCreditHours { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public bool IsApprovedByBtecAuthority { get; set; }
        public DateTime SemesterStartDate { get; set; }
        public double MinGPA { get; set; }
    }
}
