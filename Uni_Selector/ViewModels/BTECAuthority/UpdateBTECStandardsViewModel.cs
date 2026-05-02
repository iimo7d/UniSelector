using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class UpdateBTECStandardsViewModel
    {
        [Required(ErrorMessage = "Update description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string UpdateDescription { get; set; }

        [Required(ErrorMessage = "Effective date is required")]
        public DateTime EffectiveDate { get; set; }

        // Level-specific updates
        public List<UpdateLevelStandardDto> LevelStandards { get; set; } = new();

        // General requirements updates
        [Range(0, 100, ErrorMessage = "Minimum GPA must be between 0 and 100")]
        public double? MinimumGPA { get; set; }

        [Range(30, 300, ErrorMessage = "Credit hours must be between 30 and 300")]
        public int? MinimumCreditHours { get; set; }

        [Range(1, 10, ErrorMessage = "Duration must be between 1 and 10 years")]
        public int? MaximumDuration { get; set; }

        // Quality standards updates
        public bool? RequireQualifiedInstructors { get; set; }
        public bool? RequireModernFacilities { get; set; }
        public bool? RequireIndustryPartnership { get; set; }

        [Range(0, 100)]
        public double? MinimumPassRate { get; set; }

        // Notification settings
        [Required(ErrorMessage = "Notify universities option is required")]
        public bool NotifyUniversities { get; set; } = true;

        public string? NotificationMessage { get; set; }
    }
}
