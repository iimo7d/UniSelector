using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminCommission
{
    public class CommissionDetailsViewModel
    {
        public int Id { get; set; }

        // Commission Info
        public CommissionMode Mode { get; set; }
        public string ModeText { get; set; }
        public string ModeDescription { get; set; }
        public decimal Percentage { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal AmountEstimated { get; set; }
        public bool Settled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CalculatedAt { get; set; }

        // Calculation Details
        public int? HoursCountUsed { get; set; }
        public decimal? RegistrationFeeUsed { get; set; }
        public decimal? HourPriceUsed { get; set; }
        public decimal? DiscountPercentApplied { get; set; }

        // Application Info
        public int ApplicationId { get; set; }
        public string ApplicationNumber { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        public DateTime ApplicationDate { get; set; }

        // Student Info
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }

        // University Info
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public string UniversityEmail { get; set; }

        // Program Info
        public string ProgramName { get; set; }
        public string Degree { get; set; }

        // Settlement Info
        public int? MonthlySettlementId { get; set; }
        public string? SettlementPeriod { get; set; }
        public bool? SettlementClosed { get; set; }
        public DateTime? SettlementClosedAt { get; set; }

        public string GetModeDescription()
        {
            return Mode switch
            {
                CommissionMode.FirstSemesterRegistration2Percent => "2% commission on first semester registration fee only",
                CommissionMode.ProgramTotalHours2Percent => "2% commission on total program hours cost",
                CommissionMode.FirstSemesterRegPlusHours2Percent => "2% commission on registration fee + planned first semester hours",
                _ => "Unknown commission mode"
            };
        }
    }
}
