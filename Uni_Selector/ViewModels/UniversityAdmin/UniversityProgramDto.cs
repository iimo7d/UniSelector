namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class UniversityProgramDto
    {
        public int Id { get; set; }
        public string ProgramNameEnglish { get; set; } = string.Empty;
        public string ProgramNameArabic { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int DurationInYears { get; set; }
        public bool IsActive { get; set; }
    }
}
