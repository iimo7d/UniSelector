using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminApplication
{
    public class ApplicationListItemViewModel
    {
        public int Id { get; set; }
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string UniversityName { get; set; }
        public string ProgramName { get; set; }
        public string Degree { get; set; }
        public ApplicationStatus Status { get; set; }
        public string StatusText { get; set; }
        public string StatusBadgeClass { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsBtec { get; set; }

        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                ApplicationStatus.Pending => "badge bg-warning-focus text-warning-600",
                ApplicationStatus.UnderReview => "badge bg-info-focus text-info-600",
                ApplicationStatus.Approved => "badge bg-success-focus text-success-600",
                ApplicationStatus.Rejected => "badge bg-danger-focus text-danger-600",
                ApplicationStatus.Enrolled => "badge bg-primary-focus text-primary-600",
                ApplicationStatus.Cancelled => "badge bg-neutral-200 text-neutral-600",
                _ => "badge bg-neutral-200 text-neutral-600"
            };
        }
    }
}
