using OfficeOpenXml.Table.PivotTable;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class ExportReportViewModel
    {
        public ExportType ExportType { get; set; }
        public ReportType ReportType { get; set; }

        // Filters (same as respective report)
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public BtecLevel? Level { get; set; }
        public string? TechnicalField { get; set; }
        public int? UniversityId { get; set; }
        public ComplianceStatus? ComplianceStatus { get; set; }
    }
}
