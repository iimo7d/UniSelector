namespace Uni_Selector.ViewModels.DiscountManagement
{
    public class ProgramDiscountStats
    {
        public string ProgramName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public int RedeemedCount { get; set; }
    }
}
