namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class UniversitiesBTECReportViewModel
    {
        // Filters
        public string? SearchTerm { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public bool? HasApprovedPrograms { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        // Data
        public List<UniversityBtecDetailDto> Universities { get; set; } = new();

        // Filter Options
        public List<string> Provinces { get; set; } = new();
        public List<string> Cities { get; set; } = new();
    }
}
