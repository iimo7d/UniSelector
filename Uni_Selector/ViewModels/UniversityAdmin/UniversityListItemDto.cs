namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class UniversityListItemDto
    {
        public int Id { get; set; }
        public string NameEnglish { get; set; } = string.Empty;
        public string NameArabic { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int RepresentativesCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusBadgeClass => IsActive
            ? "bg-success-focus text-success-600 border border-success-main"
            : "bg-neutral-200 text-neutral-600 border border-neutral-400";

        public string StatusText => IsActive ? "Active" : "Inactive";
    }
}
