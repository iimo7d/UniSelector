namespace Uni_Selector.ViewModels.AdminProgram
{
    public class ProgramImportItemViewModel
    {
        public string NameEnglish { get; set; }
        public string NameArabic { get; set; }
        public string Degree { get; set; }
        public string Language { get; set; }
        public int TotalCreditHours { get; set; }
        public string? Reason { get; set; } // For skipped items
    }
}
