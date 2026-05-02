namespace Uni_Selector.ViewModels.UniversityManagement
{
    public class UniversityDetailsViewModel
    {
        public int Id { get; set; }
        public string NameEnglish { get; set; }
        public string NameArabic { get; set; }
        public string Type { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AcademicAccreditation { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? OfficialWebsite { get; set; }
        public string? LogoPath { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public string CommissionMode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Statistics
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int TotalRepresentatives { get; set; }
        public List<ApplicationStatusStatDto> ApplicationStats { get; set; } = new();

        // Programs
        public List<UniversityProgramInfoDto> UniversityPrograms { get; set; } = new();
        public List<BtecProgramInfoDto> BtecPrograms { get; set; } = new();

        // Representatives
        public List<RepresentativeInfoDto> Representatives { get; set; } = new();

        // Permissions
        public bool CanManagePrograms { get; set; }
        public bool CanManageFees { get; set; }
        public bool CanViewApplications { get; set; }
    }
}
