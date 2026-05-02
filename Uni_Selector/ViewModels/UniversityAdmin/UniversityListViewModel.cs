namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class UniversityListViewModel
    {
        public List<UniversityListItemDto> Universities { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? Province { get; set; }
        public bool? IsActive { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalUniversities { get; set; }
        public List<string> Provinces { get; set; } = new();
    }
}
