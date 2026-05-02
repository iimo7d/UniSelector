namespace Uni_Selector.ViewModels.Home
{
    public class UniversityCardViewModel
    {
        public int Id { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string LogoPath { get; set; }
        public string ImagePath { get; set; }
        public int ProgramsCount { get; set; }
        public int BTECProgramsCount { get; set; }
        public decimal MinHourPrice { get; set; }
        public bool IsActive { get; set; }
    }
}
