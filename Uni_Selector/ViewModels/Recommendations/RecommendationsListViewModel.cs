namespace Uni_Selector.ViewModels.Recommendations
{
    public class RecommendationsListViewModel
    {
        public List<RecommendationItemViewModel> Recommendations { get; set; } = new List<RecommendationItemViewModel>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Filters
        public string SearchTerm { get; set; }
        public string UniversityFilter { get; set; }
        public string LanguageFilter { get; set; }
        public string SortBy { get; set; } = "score";
    }
}
