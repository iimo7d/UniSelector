using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Applications
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
        public string StudentFullName { get; set; } // Alias for StudentName
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
        public string? UniversityLogo { get; set; }
        public string? UniversityAddress { get; set; }
        public string? UniversityPhone { get; set; }
        public string? UniversityEmail { get; set; }
        public string? UniversityWebsite { get; set; }

        public string ProgramNameEnglish { get; set; }
        public string ProgramName { get; set; } // Alias for ProgramNameEnglish
        public string? ProgramNameArabic { get; set; }
        public string? ProgramDescription { get; set; }
        public string Degree { get; set; }
        public string? BtecLevel { get; set; } // For BTEC programs
        public string Language { get; set; }
        public string StudySystem { get; set; }
        public int? DurationInYears { get; set; }
        public int? DurationYears { get; set; } // Alias for DurationInYears
        public int? TotalCreditHours { get; set; }
        public string? TechnicalField { get; set; }
        public DateTime? SemesterStartDate { get; set; }

        // Financial Info
        public decimal? HourPriceBase { get; set; }
        public decimal? RegistrationFeeFirstSemester { get; set; }
        public decimal? HourDiscountPercent { get; set; }
        public string? HourDiscountSetBy { get; set; }
        public DateTime? HourDiscountSetAt { get; set; }
        public int? PlannedFirstSemesterHours { get; set; }
        public decimal EstimatedTotalCost { get; set; }

        // Discount Grant Info
        public bool HasDiscountGrant { get; set; }
        public bool HasDiscount => HasDiscountGrant; // Alias
        public string? DiscountCode { get; set; }
        public DiscountStatus? DiscountStatus { get; set; }
        public DateTime? DiscountGrantedAt { get; set; }
        public DateTime? DiscountRedeemedAt { get; set; }
        public decimal? DiscountPercentage { get; set; } // Usually 5%
        public decimal? DiscountAmount { get; set; } // Calculated amount

        // Commission Info
        public bool HasCommission { get; set; }
        public decimal? CommissionAmount { get; set; }
        public bool? CommissionSettled { get; set; }

        // Helper Properties
        public string StatusBadgeClass => Status switch
        {
            ApplicationStatus.Pending => "bg-yellow text-whiteColor",
            ApplicationStatus.UnderReview => "bg-blue text-whiteColor",
            ApplicationStatus.Approved => "bg-greencolor2 text-whiteColor",
            ApplicationStatus.Rejected => "bg-red text-whiteColor",
            ApplicationStatus.Enrolled => "bg-purple text-whiteColor",
            ApplicationStatus.Cancelled => "bg-gray-500 text-whiteColor",
            _ => "bg-gray-400 text-whiteColor"
        };

        public string StatusIcon => Status switch
        {
            ApplicationStatus.Pending => "icofont-clock-time",
            ApplicationStatus.UnderReview => "icofont-eye",
            ApplicationStatus.Approved => "icofont-check-circled",
            ApplicationStatus.Rejected => "icofont-close-circled",
            ApplicationStatus.Enrolled => "icofont-graduate",
            ApplicationStatus.Cancelled => "icofont-ban",
            _ => "icofont-question-circle"
        };

        public bool CanCancel => Status == ApplicationStatus.Pending || Status == ApplicationStatus.UnderReview;
    }
}
