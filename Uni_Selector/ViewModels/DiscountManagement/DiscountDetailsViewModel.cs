using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.DiscountManagement
{
    public class DiscountDetailsViewModel
    {
        // Discount Info
        public int DiscountId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public decimal AmountEstimated { get; set; }
        public DiscountStatus Status { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public string? RedeemedByUserName { get; set; }

        // Application Info
        public int ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string? AdmissionNumber { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Student Info
        public int StudentId { get; set; }
        public string StudentUserId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentPhone { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public double GPA { get; set; }
        public PathType Path { get; set; }

        // Program Info
        public bool IsBtecProgram { get; set; }
        public string ProgramNameEnglish { get; set; } = string.Empty;
        public string ProgramNameArabic { get; set; } = string.Empty;
        public string? ProgramDescription { get; set; }
        public Degree? Degree { get; set; }
        public BtecLevel? BtecLevel { get; set; }
        public LanguageCode ProgramLanguage { get; set; }
        public int ProgramDuration { get; set; }
        public int TotalCreditHours { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal RegistrationFeeRegularSemester { get; set; }

        // Commission Info (if exists)
        public bool HasCommission { get; set; }
        public decimal? CommissionPercentage { get; set; }
        public decimal? CommissionAmountEstimated { get; set; }
        public CommissionMode? CommissionMode { get; set; }
        public bool? CommissionSettled { get; set; }

        // University Info
        public int UniversityId { get; set; }
        public string UniversityNameEnglish { get; set; } = string.Empty;
        public string UniversityNameArabic { get; set; } = string.Empty;
        public string UniversityCity { get; set; } = string.Empty;

        // Permissions
        public bool CanRedeem { get; set; }
    }
}
