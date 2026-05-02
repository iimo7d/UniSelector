using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.Applications
{
    public class CreateApplicationViewModel
    {
        public int? RecommendationId { get; set; }

        [Display(Name = "University Program")]
        public int? UniversityProgramId { get; set; }

        [Display(Name = "BTEC Program")]
        public int? BtecProgramId { get; set; }

        [Display(Name = "Planned First Semester Hours")]
        [Range(12, 18, ErrorMessage = "First semester hours must be between 12 and 18")]
        public int? PlannedFirstSemesterHours { get; set; } = 15;

        [Display(Name = "Additional Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        // Display properties (not submitted)
        public string? ProgramName { get; set; }
        public string? UniversityName { get; set; }
        public string? UniversityLogo { get; set; }
        public decimal? EstimatedCost { get; set; }
        public bool IsBtec { get; set; }
        public string? StudentFullName { get; set; }
        public string? StudentEmail { get; set; }
        public double? StudentGPA { get; set; }
    }

}
