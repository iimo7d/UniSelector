using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class EditRepresentativeViewModel
    {
        public int Id { get; set; }

        public int UniversityId { get; set; }

        
        public string UserId { get; set; } = string.Empty;

        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
        [Display(Name = "Position")]
        public string? Position { get; set; }

        [Display(Name = "Can Manage Programs")]
        public bool CanManagePrograms { get; set; }

        [Display(Name = "Can Manage Fees")]
        public bool CanManageFees { get; set; }

        [Display(Name = "Can View Applications")]
        public bool CanViewApplications { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
