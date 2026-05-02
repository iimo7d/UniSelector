namespace Uni_Selector.ViewModels.AdminCommission
{
    public class SettlementListViewModel
    {
        public List<SettlementListItemViewModel> Settlements { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalSettlements { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalSettlements / PageSize);

        // Filters
        public int? UniversityId { get; set; }
        public bool? Closed { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }

        // Summary
        public decimal TotalSettlementAmount { get; set; }
        public decimal ClosedAmount { get; set; }
        public decimal OpenAmount { get; set; }
        public int ClosedCount { get; set; }
        public int OpenCount { get; set; }

        // Filter options
        public List<UniversityFilterOption> Universities { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
    }

}
