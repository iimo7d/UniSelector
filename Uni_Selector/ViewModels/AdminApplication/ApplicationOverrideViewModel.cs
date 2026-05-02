using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminApplication
{
    public class ApplicationOverrideViewModel
    {
        public int ApplicationId { get; set; }
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string UniversityName { get; set; }
        public string ProgramName { get; set; }
        public ApplicationStatus CurrentStatus { get; set; }

        [Required(ErrorMessage = "Please select a new status")]
        [Display(Name = "New Status")]
        public ApplicationStatus NewStatus { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Override Notes")]
        public string? OverrideNotes { get; set; }

        [StringLength(200, ErrorMessage = "Admission number cannot exceed 200 characters")]
        [Display(Name = "Admission Number")]
        public string? AdmissionNumber { get; set; }

        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }
    }
}
