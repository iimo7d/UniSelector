using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class SuspendUniversityViewModel
    {
        [Required]
        public int Id { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [Display(Name = "Suspension Reason")]
        public string? Reason { get; set; }
    }
}
