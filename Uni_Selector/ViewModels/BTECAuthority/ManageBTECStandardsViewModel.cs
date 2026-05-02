namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class ManageBTECStandardsViewModel
    {
        // Update Information
        public string? UpdateDescription { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? NotificationMessage { get; set; }

        // Level-specific standards
        public List<LevelStandardDto> LevelStandards { get; set; } = new();

        // General requirements (flat properties)
        public double? MinimumGPA { get; set; }
        public int? MinimumCreditHours { get; set; }
        public int? MaximumDuration { get; set; }
        public bool RequireEnglishProficiency { get; set; }
        public bool RequireTechnicalBackground { get; set; }

        // Quality standards (flat properties)
        public bool RequireQualifiedInstructors { get; set; }
        public bool RequireModernFacilities { get; set; }
        public bool RequireIndustryPartnership { get; set; }
        public double? MinimumPassRate { get; set; }
        public bool RequireAnnualReview { get; set; }

        // Accreditation criteria
        public List<AccreditationCriteriaDto> AccreditationCriteria { get; set; } = new();

        // Notification settings
        public bool NotifyUniversities { get; set; } = true;
    }

}
