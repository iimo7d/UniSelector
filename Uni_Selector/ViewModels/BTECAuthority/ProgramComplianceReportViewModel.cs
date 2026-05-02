using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class ProgramComplianceReportViewModel
    {
        // Filters
        public string? SearchTerm { get; set; }
        public BtecLevel? Level { get; set; }
        public string? TechnicalField { get; set; }
        public int? UniversityId { get; set; }
        public ComplianceStatus? ComplianceStatus { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        // Data
        public List<ProgramComplianceDto> Programs { get; set; } = new();

        // Filter Options
        public List<string> TechnicalFields { get; set; } = new();
        public List<UniversityOptionDto> Universities { get; set; } = new();
    }
}
