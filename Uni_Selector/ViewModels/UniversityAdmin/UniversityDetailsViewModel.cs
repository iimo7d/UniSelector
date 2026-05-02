namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class UniversityDetailsViewModel
    {
        public int Id { get; set; }
        public string NameEnglish { get; set; } = string.Empty;
        public string NameArabic { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AcademicAccreditation { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? OfficialWebsite { get; set; }
        public string? LogoPath { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public string CommissionMode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Statistics
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int TotalRepresentatives { get; set; }
        public int ActiveRepresentatives { get; set; }

        // Related data
        public List<UniversityProgramDto> UniversityPrograms { get; set; } = new();
        public List<UniversityRepresentativeDto> Representatives { get; set; } = new();
        public List<ApplicationStatusDto> ApplicationStats { get; set; } = new();
    }
}
