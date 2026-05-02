using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.ApplicationReview
{
    public class RejectApplicationViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }
    }
}
