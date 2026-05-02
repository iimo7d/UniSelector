using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.ApplicationReview
{
    public class ApproveApplicationViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? ApprovalNotes { get; set; }
    }
}
