using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Commission
{
    public class CommissionListItemDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string ApplicationNumber { get; set; } = string.Empty;
        public string? AdmissionNumber { get; set; }
        public CommissionMode Mode { get; set; }
        public decimal Percentage { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal AmountEstimated { get; set; }
        public bool Settled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CalculatedAt { get; set; }
        public int? MonthlySettlementId { get; set; }
        public string? SettlementPeriod { get; set; } // "January 2024"
    }
}
