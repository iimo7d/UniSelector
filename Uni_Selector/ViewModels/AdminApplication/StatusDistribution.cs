using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminApplication
{
    public class StatusDistribution
    {
        public ApplicationStatus Status { get; set; }
        public string StatusName { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; }

        public string GetColor()
        {
            return Status switch
            {
                ApplicationStatus.Pending => "#FFA500",      // Orange
                ApplicationStatus.UnderReview => "#17A2B8",  // Info Blue
                ApplicationStatus.Approved => "#28A745",     // Success Green
                ApplicationStatus.Rejected => "#DC3545",     // Danger Red
                ApplicationStatus.Enrolled => "#487FFF",     // Primary Blue
                ApplicationStatus.Cancelled => "#6C757D",    // Neutral Gray
                _ => "#6C757D"
            };
        }
    }
}
