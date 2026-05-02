namespace Uni_Selector.ViewModels.Discounts
{
    public class DiscountsListViewModel
    {
        public List<DiscountItemViewModel> Discounts { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? StatusFilter { get; set; }

        // Helper properties for pagination UI
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int StartItem => Math.Min((PageNumber - 1) * PageSize + 1, TotalCount);
        public int EndItem => Math.Min(PageNumber * PageSize, TotalCount);

        // Get page numbers to display (with ellipsis logic)
        public List<int> GetVisiblePageNumbers(int maxVisible = 5)
        {
            var pages = new List<int>();
            var halfVisible = maxVisible / 2;
            var startPage = Math.Max(1, PageNumber - halfVisible);
            var endPage = Math.Min(TotalPages, startPage + maxVisible - 1);

            // Adjust start if we're near the end
            if (endPage - startPage < maxVisible - 1)
            {
                startPage = Math.Max(1, endPage - maxVisible + 1);
            }

            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }

            return pages;
        }
    }
}
