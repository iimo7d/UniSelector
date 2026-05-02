namespace Uni_Selector.ViewModels.Applications
{
    public class ApplicationsListViewModel
    {
        public List<ApplicationItemViewModel> Applications { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "newest";
    }
}
