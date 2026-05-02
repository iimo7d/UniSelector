using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.DiscountManagement
{
    public class DiscountListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string ApplicationNumber { get; set; } = string.Empty;
        public string? AdmissionNumber { get; set; }
        public decimal Percentage { get; set; }
        public decimal AmountEstimated { get; set; }
        public DiscountStatus Status { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public string? RedeemedByUserName { get; set; }
    }
}
