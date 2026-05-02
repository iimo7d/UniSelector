using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminApplication
{
    public class ApplicationListViewModel
    {
        public List<ApplicationListItemViewModel> Applications { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalApplications { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalApplications / PageSize);

        // Filters
        public string? SearchTerm { get; set; }
        public ApplicationStatus? Status { get; set; }
        public int? UniversityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Filter options
        public List<ApplicationStatus> AllStatuses { get; set; } = new();
        public List<UniversityFilterOption> Universities { get; set; } = new();
    }
}
