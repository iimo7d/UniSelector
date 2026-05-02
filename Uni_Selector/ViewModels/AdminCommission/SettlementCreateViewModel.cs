using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.AdminCommission
{
    public class SettlementCreateViewModel
    {
        [Required(ErrorMessage = "Please select a university")]
        [Display(Name = "University")]
        public int UniversityId { get; set; }

        [Required(ErrorMessage = "Please select a year")]
        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100")]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please select a month")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        [Display(Name = "Month")]
        public int Month { get; set; }

        // For displaying available universities
        public List<UniversityOption> Universities { get; set; } = new();

        // Preview data before creation
        public SettlementPreviewData? Preview { get; set; }
    }
}
