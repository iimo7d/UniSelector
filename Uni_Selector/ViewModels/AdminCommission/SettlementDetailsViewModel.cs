namespace Uni_Selector.ViewModels.AdminCommission
{
    public class SettlementDetailsViewModel
    {
        public int Id { get; set; }

        // Settlement Info
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public string UniversityEmail { get; set; }
        public string UniversityPhone { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Period { get; set; }
        public decimal TotalCommission { get; set; }
        public int StudentCount { get; set; }
        public bool Closed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedByUserName { get; set; }

        // Commission Breakdown
        public List<CommissionBreakdownItem> Commissions { get; set; } = new();

        // Statistics
        public decimal AverageCommissionPerStudent { get; set; }
        public decimal HighestCommission { get; set; }
        public decimal LowestCommission { get; set; }

        // Commission Mode Distribution
        public Dictionary<string, int> ModeDistribution { get; set; } = new();
        public Dictionary<string, decimal> ModeAmounts { get; set; } = new();

        public string GetPeriod()
        {
            return new DateTime(Year, Month, 1).ToString("MMMM yyyy");
        }
    }
}
