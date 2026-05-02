using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class AddEntryRequirementViewModel
    {
        [Required(ErrorMessage = "Minimum GPA is required")]
        [Range(50, 100, ErrorMessage = "GPA must be between 50 and 100")]
        [Display(Name = "Minimum GPA (%)")]
        public double MinGPA { get; set; }

        [Required(ErrorMessage = "Educational path is required")]
        [Display(Name = "Educational Path")]
        public PathType Path { get; set; }

        [Display(Name = "Academic Track")]
        public AcademicTrack? AcademicTrack { get; set; }

        [Display(Name = "Vocational Branch")]
        public VocationalBranch? VocationalBranch { get; set; }

        [Display(Name = "Allow No Tawjihi")]
        public bool AllowNoTawjihi { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Effective From")]
        public DateTime? EffectiveFrom { get; set; }
    }
}
