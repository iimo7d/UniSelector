using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Discounts
{
    public class DiscountItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public decimal AmountEstimated { get; set; }
        public DiscountStatus Status { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public string UniversityName { get; set; } = string.Empty;
        public string? UniversityLogo { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string? ApplicationNumber { get; set; }
        public bool IsValid { get; set; }

        // =========================================================
        // ENHANCED UI HELPERS
        // =========================================================

        public int DaysRemaining => IsValid ? Math.Max(0, (GrantedAt.AddDays(90) - DateTime.UtcNow).Days) : 0;
        public DateTime ExpiryDate => GrantedAt.AddDays(90);

        // Effective status accounting for expired-but-not-marked discounts
        public DiscountStatus EffectiveStatus =>
            Status == DiscountStatus.Issued && DaysRemaining <= 0 ? DiscountStatus.Expired : Status;

        // Status badge styling for cards
        public string StatusBadgeClass => EffectiveStatus switch
        {
            DiscountStatus.Issued => "bg-greencolor2 text-whiteColor shadow-lg shadow-greencolor2/20 ring-2 ring-greencolor2/20",
            DiscountStatus.Redeemed => "bg-blue text-whiteColor shadow-lg shadow-blue/20 ring-2 ring-blue/20",
            DiscountStatus.Expired => "bg-gray-600 text-whiteColor shadow-lg shadow-gray-500/20 ring-2 ring-gray-500/20",
            _ => "bg-gray-500 text-whiteColor shadow-lg shadow-gray-500/20"
        };

        // Icon for status
        public string StatusIcon => EffectiveStatus switch
        {
            DiscountStatus.Issued => "icofont-check-circled",
            DiscountStatus.Redeemed => "icofont-tick-mark",
            DiscountStatus.Expired => "icofont-close-circled",
            _ => "icofont-question-circle"
        };

        // Card border accent based on status
        public string CardBorderClass => EffectiveStatus switch
        {
            DiscountStatus.Issued => "border-l-4 border-l-greencolor2",
            DiscountStatus.Redeemed => "border-l-4 border-l-blue",
            DiscountStatus.Expired => "border-l-4 border-l-gray-500",
            _ => "border-l-4 border-l-gray-400"
        };

        // Progress percentage (for progress bars if needed)
        public int ProgressPercentage
        {
            get
            {
                if (!IsValid || Status != DiscountStatus.Issued) return 0;
                var totalDays = 90;
                var remainingDays = DaysRemaining;
                return Math.Max(0, Math.Min(100, (remainingDays * 100) / totalDays));
            }
        }

        // Urgency level for UI decisions
        public string UrgencyLevel
        {
            get
            {
                if (!IsValid || Status != DiscountStatus.Issued) return "none";
                if (DaysRemaining <= 7) return "critical";
                if (DaysRemaining <= 30) return "warning";
                return "normal";
            }
        }

        // Urgency color class
        public string UrgencyColorClass => UrgencyLevel switch
        {
            "critical" => "text-red-600 dark:text-red-400",
            "warning" => "text-yellow-600 dark:text-yellow-400",
            "normal" => "text-greencolor2",
            _ => "text-gray-500"
        };
    }
}
