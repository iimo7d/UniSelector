using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Commission
{
    public class CommissionDetailsViewModel
    {
        // Commission Info
        public int CommissionId { get; set; }
        public CommissionMode Mode { get; set; }
        public decimal Percentage { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal AmountEstimated { get; set; }
        public int? HoursCountUsed { get; set; }
        public decimal? RegistrationFeeUsed { get; set; }
        public decimal? HourPriceUsed { get; set; }
        public decimal? DiscountPercentApplied { get; set; }
        public bool Settled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CalculatedAt { get; set; }

        // Settlement Info (if settled)
        public int? MonthlySettlementId { get; set; }
        public string? SettlementPeriod { get; set; } // "January 2024"
        public DateTime? SettlementClosedAt { get; set; }
        public string? SettlementClosedByUserName { get; set; }

        // Application Info
        public int ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string? AdmissionNumber { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Student Info
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentPhone { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public double GPA { get; set; }
        public PathType Path { get; set; }

        // Program Info
        public bool IsBtecProgram { get; set; }
        public string ProgramNameEnglish { get; set; } = string.Empty;
        public string ProgramNameArabic { get; set; } = string.Empty;
        public Degree? Degree { get; set; }
        public BtecLevel? BtecLevel { get; set; }
        public int TotalCreditHours { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal RegistrationFeeRegularSemester { get; set; }

        // Discount Info (if exists)
        public bool HasDiscount { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmountEstimated { get; set; }
        public DiscountStatus? DiscountStatus { get; set; }

        // University Info
        public int UniversityId { get; set; }
        public string UniversityNameEnglish { get; set; } = string.Empty;
        public string UniversityNameArabic { get; set; } = string.Empty;
    }
}
