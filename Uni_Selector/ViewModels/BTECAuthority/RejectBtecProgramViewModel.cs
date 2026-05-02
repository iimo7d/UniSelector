using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class RejectBtecProgramViewModel
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string RejectionReason { get; set; }
    }
}
