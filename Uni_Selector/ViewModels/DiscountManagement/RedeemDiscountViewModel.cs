using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.DiscountManagement
{

    public class RedeemDiscountViewModel
    {
        [Required]
        public int DiscountId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
