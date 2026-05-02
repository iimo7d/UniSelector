using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class ApproveBtecProgramViewModel
    {
        [Required(ErrorMessage = "Approval notes are required")]
        [StringLength(500, ErrorMessage = "Approval notes cannot exceed 500 characters")]
        public string ApprovalNotes { get; set; }
    }
}
