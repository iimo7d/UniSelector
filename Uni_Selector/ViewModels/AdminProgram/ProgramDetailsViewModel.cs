namespace Uni_Selector.ViewModels.AdminProgram
{
    public class ProgramDetailsViewModel
    {
        public int Id { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
        public string? Description { get; set; }
        public string DegreeText { get; set; }
        public string LanguageText { get; set; }
        public string? AcademicClassification { get; set; }
        public int TotalCreditHours { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<UniversityOfferingViewModel> UniversitiesOffering { get; set; } = new();
    }
}
