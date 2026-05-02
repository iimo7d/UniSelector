using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ApplicationReview
{
    public class ApplicationListViewModel
    {
        public List<ApplicationListItemDto> Applications { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? Search { get; set; }
        public ApplicationStatus? StatusFilter { get; set; }
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public bool CanViewApplications { get; set; }
    }
}
