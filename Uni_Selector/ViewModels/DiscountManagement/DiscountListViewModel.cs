using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.DiscountManagement
{
    public class DiscountListViewModel
    {
        public List<DiscountListItemDto> Discounts { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Filters
        public string? Search { get; set; }
        public DiscountStatus? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Context
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = string.Empty;
        public bool CanViewDiscounts { get; set; }
    }
}
