using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Admin
{
    public class StatusCountDto
    {
        public ApplicationStatus Status { get; set; }
        public int Count { get; set; }

        public string StatusName => Status.ToString();

        public string Color => Status switch
        {
            ApplicationStatus.Pending => "#FFA500",
            ApplicationStatus.UnderReview => "#17A2B8",
            ApplicationStatus.Approved => "#28A745",
            ApplicationStatus.Rejected => "#DC3545",
            ApplicationStatus.Enrolled => "#007BFF",
            ApplicationStatus.Cancelled => "#6C757D",
            _ => "#6C757D"
        };
    }
}
