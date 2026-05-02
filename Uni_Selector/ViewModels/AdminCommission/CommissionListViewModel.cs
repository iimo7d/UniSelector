using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.AdminApplication;

namespace Uni_Selector.ViewModels.AdminCommission
{
    public class CommissionListViewModel
    {
        public List<CommissionListItemViewModel> Commissions { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCommissions { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCommissions / PageSize);

        // Filters
        public int? UniversityId { get; set; }
        public bool? Settled { get; set; }
        public CommissionMode? Mode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Summary
        public decimal TotalCommissionAmount { get; set; }
        public decimal SettledAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int SettledCount { get; set; }
        public int PendingCount { get; set; }

        // Filter options
        public List<UniversityFilterOption> Universities { get; set; } = new();
        public List<CommissionMode> AllModes { get; set; } = new();
    }
}
