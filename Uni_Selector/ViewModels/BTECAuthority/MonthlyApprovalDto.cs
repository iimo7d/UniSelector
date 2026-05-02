namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class MonthlyApprovalDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
    }
}
