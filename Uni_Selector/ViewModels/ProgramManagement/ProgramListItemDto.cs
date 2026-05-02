using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class ProgramListItemDto
    {
        public int Id { get; set; }
        public string ProgramNameArabic { get; set; }
        public string ProgramNameEnglish { get; set; }
        public Degree Degree { get; set; }
        public LanguageCode Language { get; set; }
        public StudySystem StudySystem { get; set; }
        public int DurationInYears { get; set; }
        public decimal HourPriceBase { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
