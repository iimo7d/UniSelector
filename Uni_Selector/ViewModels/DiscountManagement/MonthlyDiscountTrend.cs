namespace Uni_Selector.ViewModels.DiscountManagement
{
    public class MonthlyDiscountTrend
    {
        public string MonthYear { get; set; } = string.Empty; // "Jan 2024"
        public int Issued { get; set; }
        public int Redeemed { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
