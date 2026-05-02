namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class UniversityBasicInfoDto
    {
        public int Id { get; set; }
        public string NameEnglish { get; set; } = string.Empty;
        public string NameArabic { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public bool IsActive { get; set; }
    }
}
