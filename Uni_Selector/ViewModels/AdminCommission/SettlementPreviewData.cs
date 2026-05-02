namespace Uni_Selector.ViewModels.AdminCommission
{
    public class SettlementPreviewData
    {
        public string UniversityName { get; set; }
        public string Period { get; set; }
        public int CommissionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int StudentCount { get; set; }
        public List<CommissionPreviewItem> Commissions { get; set; } = new();
    }
}
