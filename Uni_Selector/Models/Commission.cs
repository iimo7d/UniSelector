using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Models
{
    public class Commission
    {
        public int Id { get; set; }
        [Required] public int ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public StudentApplication Application { get; set; }
        [Required] public int UniversityId { get; set; }
        [ForeignKey("UniversityId")]
        public University University { get; set; }
        [Required] public CommissionMode Mode { get; set; }
        [Required, Range(0, 100), Column(TypeName = "decimal(5,2)")] public decimal Percentage { get; set; } = 2m;
        [Required, Column(TypeName = "decimal(18,2)")] public decimal BaseAmount { get; set; }
        public int? HoursCountUsed { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? RegistrationFeeUsed { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? HourPriceUsed { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal? DiscountPercentApplied { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] public decimal AmountEstimated { get; set; }
        public bool Settled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CalculatedAt { get; set; }
        public int? MonthlySettlementId { get; set; }
        [ForeignKey("MonthlySettlementId")]
        public MonthlySettlement? MonthlySettlement { get; set; }
    }
}
