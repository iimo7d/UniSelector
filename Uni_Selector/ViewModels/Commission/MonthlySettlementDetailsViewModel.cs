using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Commission
{
    public class MonthlySettlementDetailsViewModel
    {
        // Settlement Info
        public int SettlementId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string PeriodDisplay { get; set; } = string.Empty; // "January 2024"
        public decimal TotalCommission { get; set; }
        public int StudentCount { get; set; }
        public bool Closed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedByUserName { get; set; }
        public string? Notes { get; set; }

        // Commissions in this settlement
        public List<CommissionListItemDto> Commissions { get; set; } = new();

        // Statistics
        public decimal AverageCommissionAmount { get; set; }
        public CommissionMode MostCommonMode { get; set; }
        public Dictionary<CommissionMode, int> CommissionsByMode { get; set; } = new();
        public Dictionary<CommissionMode, decimal> AmountByMode { get; set; } = new();

        // University Info
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = string.Empty;
    }

}
