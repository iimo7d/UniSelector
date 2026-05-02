namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class BtecProgramListViewModel
    {
        public List<BtecProgramListItemDto> Programs { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? Search { get; set; }
        public bool? IsActiveFilter { get; set; }
        public bool? IsApprovedFilter { get; set; }
        public int UniversityId { get; set; }
        public bool CanManagePrograms { get; set; }
    }
}
