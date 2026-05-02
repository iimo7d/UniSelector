using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminCommission
{
    public class CommissionListItemViewModel
    {
        public int Id { get; set; }
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string UniversityName { get; set; }
        public CommissionMode Mode { get; set; }
        public string ModeText { get; set; }
        public decimal Percentage { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal AmountEstimated { get; set; }
        public bool Settled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CalculatedAt { get; set; }
        public int? MonthlySettlementId { get; set; }
        public string? SettlementPeriod { get; set; }
    }
}
