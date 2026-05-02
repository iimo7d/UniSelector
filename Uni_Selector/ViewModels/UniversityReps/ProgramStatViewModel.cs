namespace Uni_Selector.ViewModels.UniversityReps
{
    public class ProgramStatViewModel
    {
        public string ProgramName { get; set; }
        public int TotalApplications { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
        public decimal ApprovalRate { get; set; }
    }

}
