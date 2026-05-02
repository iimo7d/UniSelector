using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class AddBtecEntryRequirementViewModel
    {
        [Required(ErrorMessage = "Minimum GPA is required")]
        [Range(50, 100, ErrorMessage = "GPA must be between 50 and 100")]
        [Display(Name = "Minimum GPA (%)")]
        public double MinGPA { get; set; }

        [Display(Name = "Requires BTEC Level 2")]
        public bool RequiresBtecL2 { get; set; } = true;

        [Display(Name = "Requires BTEC Level 3")]
        public bool RequiresBtecL3 { get; set; } = true;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Effective From")]
        public DateTime? EffectiveFrom { get; set; }
    }
}
