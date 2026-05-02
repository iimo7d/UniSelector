using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.ApplicationReview
{
    public class SetDiscountViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "Hour discount percentage is required")]
        [Range(30, 50, ErrorMessage = "Discount must be between 30% and 50%")]
        [Display(Name = "Hour Discount Percentage")]
        public decimal HourDiscountPercent { get; set; }

        [Required(ErrorMessage = "Planned first semester hours is required")]
        [Range(1, 60, ErrorMessage = "Hours must be between 1 and 60")]
        [Display(Name = "Planned First Semester Hours")]
        public int PlannedFirstSemesterHours { get; set; }
    }
}
