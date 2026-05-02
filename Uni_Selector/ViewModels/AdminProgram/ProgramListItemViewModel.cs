namespace Uni_Selector.ViewModels.AdminProgram
{
    public class ProgramListItemViewModel
    {
        public int Id { get; set; }
        public string NameEnglish { get; set; }
        public string NameArabic { get; set; }
        public string DegreeText { get; set; }
        public string LanguageText { get; set; }
        public string? AcademicClassification { get; set; }
        public int TotalCreditHours { get; set; }
        public int UniversitiesCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DegreeBadgeClass => DegreeText switch
        {
            "Diploma" => "badge bg-info-focus text-info-600",
            "Bachelor" => "badge bg-success-focus text-success-600",
            "Master" => "badge bg-warning-focus text-warning-600",
            "PhD" => "badge bg-danger-focus text-danger-600",
            _ => "badge bg-neutral-200 text-neutral-600"
        };

        public string LanguageBadgeClass => LanguageText switch
        {
            "English" => "badge bg-primary-focus text-primary-600",
            "Arabic" => "badge bg-success-focus text-success-600",
            "Both" => "badge bg-purple-focus text-purple-600",
            _ => "badge bg-neutral-200 text-neutral-600"
        };
    }
}
