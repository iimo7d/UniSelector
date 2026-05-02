using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BtecProgramListViewModel
    {
        public List<BtecProgramListItemDto> Programs { get; set; } = new();

        // Pagination
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Filters
        public string? Search { get; set; }
        public BtecLevel? LevelFilter { get; set; }
        public string? TechnicalFieldFilter { get; set; }
        public bool? ApprovalStatusFilter { get; set; }
        public int? UniversityIdFilter { get; set; }

        // Filter Options
        public List<UniversityOptionDto> Universities { get; set; } = new();
        public List<string> TechnicalFields { get; set; } = new();
    }
}
