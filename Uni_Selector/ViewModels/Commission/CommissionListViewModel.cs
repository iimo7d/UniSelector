using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Commission
{
    public class CommissionListViewModel
    {
        public List<CommissionListItemDto> Commissions { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Filters
        public string? Search { get; set; }
        public bool? SettledFilter { get; set; }
        public CommissionMode? ModeFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Summary Stats
        public decimal TotalCommissionAmount { get; set; }
        public decimal SettledAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int SettledCount { get; set; }
        public int PendingCount { get; set; }

        // Context
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = string.Empty;
        public bool CanViewCommissions { get; set; }
    }
}
