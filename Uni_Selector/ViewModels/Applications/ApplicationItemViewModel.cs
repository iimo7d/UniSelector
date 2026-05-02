using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Applications
{
    public class ApplicationItemViewModel
    {
        public int Id { get; set; }
        public string? ApplicationNumber { get; set; }
        public string? AdmissionNumber { get; set; }
        public string ProgramName { get; set; }
        public string ProgramNameArabic { get; set; }
        public string UniversityName { get; set; }
        public string? UniversityLogo { get; set; }
        public string? UniversityImage { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsBtec { get; set; }
        public bool HasDiscount { get; set; }
        public string? DiscountCode { get; set; }
        public bool CanCancel { get; set; }
    }
}
