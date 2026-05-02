namespace Uni_Selector.ViewModels.DiscountManagement
{
    public class DiscountStatisticsViewModel
    {
        // Overview Stats
        public int TotalDiscountsIssued { get; set; }
        public int TotalDiscountsRedeemed { get; set; }
        public int TotalDiscountsExpired { get; set; }
        public int TotalDiscountsPending { get; set; }
        public decimal TotalAmountIssued { get; set; }
        public decimal TotalAmountRedeemed { get; set; }
        public decimal AverageDiscountPercentage { get; set; }

        // Status Breakdown (for Pie Chart)
        public Dictionary<string, int> DiscountsByStatus { get; set; } = new();
        public Dictionary<string, decimal> AmountByStatus { get; set; } = new();

        // Monthly Trends (for Line Chart - last 12 months)
        public List<MonthlyDiscountTrend> MonthlyTrends { get; set; } = new();

        // Program Distribution (for Column Chart - top 10 programs)
        public List<ProgramDiscountStats> ProgramStats { get; set; } = new();

        // Recent Activity
        public List<DiscountListItemDto> RecentDiscounts { get; set; } = new();

        // Context
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = string.Empty;
    }
}
