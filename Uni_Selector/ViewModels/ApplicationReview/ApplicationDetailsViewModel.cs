using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ApplicationReview
{
    public class ApplicationDetailsViewModel
    {
        // Application Info
        public int ApplicationId { get; set; }
        public string ApplicationNumber { get; set; }
        public string? AdmissionNumber { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
        public int? PlannedFirstSemesterHours { get; set; }
        public decimal? HourDiscountPercent { get; set; }
        public string? HourDiscountSetByUserName { get; set; }
        public DateTime? HourDiscountSetAt { get; set; }

        // Student Info
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentPhone { get; set; }
        public string NationalId { get; set; }
        public string SeatNumber { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public double GPA { get; set; }
        public PathType Path { get; set; }
        public AcademicTrack? AcademicTrack { get; set; }
        public VocationalBranch? VocationalBranch { get; set; }
        public bool BtecLevel2Completed { get; set; }
        public bool BtecLevel3Completed { get; set; }
        public string? BtecCertificateUrl { get; set; }

        // Guardian Info
        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianRelation { get; set; }

        // Location
        public string Province { get; set; }
        public string City { get; set; }
        public string? Area { get; set; }

        // Preferences
        public decimal RegistrationBudget { get; set; }
        public string? DesiredMajors { get; set; }
        public string? PreferredCity { get; set; }
        public LanguageCode PreferredLanguage { get; set; }
        public bool HasFamilyConnection { get; set; }
        public string? FamilyConnectionUniversityName { get; set; }

        // Program Info
        public bool IsBtecProgram { get; set; }
        public string ProgramNameArabic { get; set; }
        public string ProgramNameEnglish { get; set; }
        public string? ProgramDescription { get; set; }
        public Degree? Degree { get; set; } // For regular programs
        public BtecLevel? BtecLevel { get; set; } // For BTEC programs
        public string? TechnicalField { get; set; } // For BTEC programs
        public LanguageCode ProgramLanguage { get; set; }
        public int ProgramDuration { get; set; }
        public int TotalCreditHours { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal RegistrationFeeRegularSemester { get; set; }
        public StudySystem? StudySystem { get; set; } // For regular programs
        public bool IsProgramApprovedByBtec { get; set; }

        // Discount & Commission
        public bool HasDiscountGrant { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmountEstimated { get; set; }
        public DiscountStatus? DiscountStatus { get; set; }
        public DateTime? DiscountGrantedAt { get; set; }

        public bool HasCommission { get; set; }
        public decimal? CommissionPercentage { get; set; }
        public decimal? CommissionAmountEstimated { get; set; }
        public CommissionMode? CommissionMode { get; set; }
        public bool? CommissionSettled { get; set; }

        // University Info
        public int UniversityId { get; set; }
        public string UniversityNameArabic { get; set; }
        public string UniversityNameEnglish { get; set; }
        public string UniversityCity { get; set; }
        public string UniversityPhone { get; set; }
        public string UniversityEmail { get; set; }

        // Permissions
        public bool CanViewApplications { get; set; }
        public bool CanApprove => CanViewApplications && Status == ApplicationStatus.Pending || Status == ApplicationStatus.UnderReview;
        public bool CanReject => CanViewApplications && Status == ApplicationStatus.Pending || Status == ApplicationStatus.UnderReview;
        public bool CanSetDiscount => CanViewApplications && (Status == ApplicationStatus.Approved || Status == ApplicationStatus.Enrolled);
        public bool CanAddNotes => CanViewApplications;
    }
}
