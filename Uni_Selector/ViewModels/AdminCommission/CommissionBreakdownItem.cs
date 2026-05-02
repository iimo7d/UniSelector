namespace Uni_Selector.ViewModels.AdminCommission
{
    public class CommissionBreakdownItem
    {
        public int CommissionId { get; set; }
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string ProgramName { get; set; }
        public string Degree { get; set; }
        public string CommissionMode { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal Percentage { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime CommissionCreatedAt { get; set; }
    }
}
