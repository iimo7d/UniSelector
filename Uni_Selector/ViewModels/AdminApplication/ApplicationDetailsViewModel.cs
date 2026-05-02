using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminApplication
{
    public class ApplicationDetailsViewModel
    {
        // Application Info
        public int Id { get; set; }
        public string ApplicationNumber { get; set; }
        public ApplicationStatus Status { get; set; }
        public string StatusText { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public string? AdmissionNumber { get; set; }
        public string? RejectionReason { get; set; }

        // Student Info
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentPhone { get; set; }
        public double StudentGPA { get; set; }
        public string StudentProvince { get; set; }
        public string StudentCity { get; set; }
        public PathType StudentPath { get; set; }
        public decimal RegistrationBudget { get; set; }

        // University/Program Info
        public bool IsBtec { get; set; }
        public string UniversityName { get; set; }
        public string ProgramNameEnglish { get; set; }
        public string ProgramNameArabic { get; set; }
        public string Degree { get; set; }
        public string Language { get; set; }
        public string StudySystem { get; set; }
        public int DurationInYears { get; set; }
        public int TotalCreditHours { get; set; }

        // Financial Info
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal? HourDiscountPercent { get; set; }
        public string? HourDiscountSetBy { get; set; }
        public DateTime? HourDiscountSetAt { get; set; }
        public int? PlannedFirstSemesterHours { get; set; }
        public decimal EstimatedTotalCost { get; set; }

        // Discount Grant Info
        public bool HasDiscountGrant { get; set; }
        public string? DiscountCode { get; set; }
        public DiscountStatus? DiscountStatus { get; set; }
        public DateTime? DiscountGrantedAt { get; set; }
        public DateTime? DiscountRedeemedAt { get; set; }

        // Commission Info
        public bool HasCommission { get; set; }
        public decimal? CommissionAmount { get; set; }
        public bool? CommissionSettled { get; set; }
    }
}
