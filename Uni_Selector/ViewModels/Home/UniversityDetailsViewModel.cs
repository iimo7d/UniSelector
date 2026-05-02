using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Home
{
    public class UniversityDetailsViewModel
    {
        // University Info
        public int Id { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
        public UniversityType Type { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string OfficialWebsite { get; set; }
        public string LogoPath { get; set; }
        public string ImagePath { get; set; }
        public string AcademicAccreditation { get; set; }
        public CommissionMode CommissionMode { get; set; }

        // Programs
        public List<UniversityProgramCardViewModel> RegularPrograms { get; set; } = new();
        public List<BTECProgramCardViewModel> BTECPrograms { get; set; } = new();

        // Statistics
        public int TotalPrograms { get; set; }
        public int TotalBTECPrograms { get; set; }
        public int TotalStudents { get; set; }
        public decimal MinHourPrice { get; set; }
        public decimal MaxHourPrice { get; set; }
    }
}
