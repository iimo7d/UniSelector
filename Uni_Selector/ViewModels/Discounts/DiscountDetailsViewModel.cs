using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Discounts
{

    public class DiscountDetailsViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public decimal AmountEstimated { get; set; }
        public DiscountStatus Status { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public string? RedeemedBy { get; set; }

        // University Info
        public string UniversityName { get; set; } = string.Empty;
        public string? UniversityNameArabic { get; set; }
        public string? UniversityLogo { get; set; }
        public string? UniversityAddress { get; set; }
        public string? UniversityPhone { get; set; }
        public string? UniversityEmail { get; set; }

        // Program Info
        public string ProgramName { get; set; } = string.Empty;
        public string? ProgramNameArabic { get; set; }

        // Application Info
        public string? ApplicationNumber { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }

        // Student Info
        public string StudentFullName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;

        // Validity Info
        public bool IsValid { get; set; }
        public int DaysUntilExpiry { get; set; }
        public DateTime ExpiryDate => GrantedAt.AddDays(90);

        // Financial Info
        public decimal? EstimatedRegistrationFee { get; set; }

        // =========================================================
        // ENHANCED UI HELPERS (use these in the Razor)
        // =========================================================

        // If it is "Issued" but time passed, treat it as Expired for UI
        public DiscountStatus EffectiveStatus =>
            Status == DiscountStatus.Issued && DaysUntilExpiry <= 0 ? DiscountStatus.Expired : Status;

        // Cleaner label for UI titles
        public string StatusTitle => EffectiveStatus switch
        {
            DiscountStatus.Issued => "Active",
            DiscountStatus.Redeemed => "Redeemed",
            DiscountStatus.Expired => "Expired",
            _ => "Unknown"
        };

        // Accent color token (text)
        public string AccentTextClass => EffectiveStatus switch
        {
            DiscountStatus.Issued => "text-greencolor2",
            DiscountStatus.Redeemed => "text-blue",
            DiscountStatus.Expired => "text-gray-600 dark:text-gray-300",
            _ => "text-gray-500"
        };

        // Accent color (solid background)
        public string AccentBgSolidClass => EffectiveStatus switch
        {
            DiscountStatus.Issued => "bg-greencolor2",
            DiscountStatus.Redeemed => "bg-blue",
            DiscountStatus.Expired => "bg-gray-500",
            _ => "bg-gray-400"
        };

        // Accent color (soft background with opacity)
        public string AccentBgSoftClass => EffectiveStatus switch
        {
            DiscountStatus.Issued => "bg-greencolor2/10",
            DiscountStatus.Redeemed => "bg-blue/10",
            DiscountStatus.Expired => "bg-gray-500/10",
            _ => "bg-gray-400/10"
        };

        // Banner (top status box) background gradient
        public string BannerGradientClass => EffectiveStatus switch
        {
            DiscountStatus.Issued => "bg-gradient-to-r from-greencolor2/12 to-greencolor2/5",
            DiscountStatus.Redeemed => "bg-gradient-to-r from-blue/12 to-blue/5",
            DiscountStatus.Expired => "bg-gradient-to-r from-gray-500/12 to-gray-500/5",
            _ => "bg-gradient-to-r from-gray-400/12 to-gray-400/5"
        };

        public string BannerBorderClass => EffectiveStatus switch
        {
            DiscountStatus.Issued => "border-greencolor2",
            DiscountStatus.Redeemed => "border-blue",
            DiscountStatus.Expired => "border-gray-500",
            _ => "border-gray-400"
        };

        // Icon (circle) classes for status display
        public string IconCircleClass =>
            $"{AccentBgSolidClass} shadow-lg ring-4 ring-whiteColor/30 dark:ring-blackColor/10";

        // Badge pill (small status pill)
        public string StatusBadgeClass => EffectiveStatus switch
        {
            DiscountStatus.Issued =>
                "bg-greencolor2 text-whiteColor shadow-lg shadow-greencolor2/20 ring-2 ring-greencolor2/20",
            DiscountStatus.Redeemed =>
                "bg-blue text-whiteColor shadow-lg shadow-blue/20 ring-2 ring-blue/20",
            DiscountStatus.Expired =>
                "bg-gray-600 text-whiteColor shadow-lg shadow-gray-500/20 ring-2 ring-gray-500/20",
            _ =>
                "bg-gray-500 text-whiteColor shadow-lg shadow-gray-500/20 ring-2 ring-gray-500/20"
        };

        // Icon choice based on status
        public string StatusIcon => EffectiveStatus switch
        {
            DiscountStatus.Issued => "icofont-check-circled",
            DiscountStatus.Redeemed => "icofont-tick-mark",
            DiscountStatus.Expired => "icofont-close-circled",
            _ => "icofont-question-circle"
        };

        // More informative message (looks better in UI)
        public string ValidityMessage => EffectiveStatus switch
        {
            DiscountStatus.Issued => DaysUntilExpiry > 0
                ? $"Valid • {DaysUntilExpiry} days left (expires {ExpiryDate:MMM dd, yyyy})"
                : $"Expired on {ExpiryDate:MMM dd, yyyy}",

            DiscountStatus.Redeemed => RedeemedAt.HasValue
                ? $"Redeemed on {RedeemedAt.Value:MMM dd, yyyy}"
                : "Redeemed",

            DiscountStatus.Expired => $"This discount code expired on {ExpiryDate:MMM dd, yyyy}",

            _ => "Status unavailable"
        };

        // Optional: use this for the ping animation (only when actually active)
        public bool ShouldPulse => EffectiveStatus == DiscountStatus.Issued && DaysUntilExpiry > 0;

        // Urgency level for warnings
        public string UrgencyLevel
        {
            get
            {
                if (!IsValid || EffectiveStatus != DiscountStatus.Issued) return "none";
                if (DaysUntilExpiry <= 7) return "critical";
                if (DaysUntilExpiry <= 30) return "warning";
                return "normal";
            }
        }

        // Urgency message
        public string UrgencyMessage => UrgencyLevel switch
        {
            "critical" => $"⚠️ This discount expires in {DaysUntilExpiry} days! Use it soon.",
            "warning" => $"This discount will expire in {DaysUntilExpiry} days.",
            "normal" => $"You have {DaysUntilExpiry} days to use this discount.",
            _ => string.Empty
        };

        // Calculate savings percentage if we have registration fee
        public decimal? SavingsPercentageOfFee
        {
            get
            {
                if (!EstimatedRegistrationFee.HasValue || EstimatedRegistrationFee.Value == 0)
                    return null;
                return Math.Round((AmountEstimated / EstimatedRegistrationFee.Value) * 100, 2);
            }
        }

        // QR Code data (can be used with a QR code generator library)
        public string GetQRCodeData()
        {
            return $"DISCOUNT:{Code}|UNIVERSITY:{UniversityName}|PERCENT:{Percentage}|EXPIRES:{ExpiryDate:yyyy-MM-dd}";
        }
    }
}
