using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.ApplicationReview
{
    public class AddNotesViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "Notes are required")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}
