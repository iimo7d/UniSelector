using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Admin
{
    public class ApplicationSummaryDto
    {
        public int Id { get; set; }
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string UniversityName { get; set; }
        public string ProgramName { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }

        public string StatusBadgeClass => Status switch
        {
            ApplicationStatus.Pending => "bg-warning-focus text-warning-main",
            ApplicationStatus.UnderReview => "bg-info-focus text-info-main",
            ApplicationStatus.Approved => "bg-success-focus text-success-main",
            ApplicationStatus.Rejected => "bg-danger-focus text-danger-main",
            ApplicationStatus.Enrolled => "bg-primary-focus text-primary-main",
            ApplicationStatus.Cancelled => "bg-neutral-200 text-neutral-600",
            _ => "bg-neutral-200 text-neutral-600"
        };

        public string StatusText => Status.ToString();
    }
}
