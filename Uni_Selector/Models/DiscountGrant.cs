using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Models
{
    public class DiscountGrant
    {
        public int Id { get; set; }
        [Required] public int ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public StudentApplication Application { get; set; }
        [Required, StringLength(100)]
        public string Code { get; set; }
        [Required] public int UniversityId { get; set; }
        [ForeignKey("UniversityId")]
        public University University { get; set; }
        [Required, Range(0, 100), Column(TypeName = "decimal(5,2)")] public decimal Percentage { get; set; } = 5m;
        [Required, Column(TypeName = "decimal(18,2)")] public decimal AmountEstimated { get; set; }
        [Required] public DiscountStatus Status { get; set; } = DiscountStatus.Issued;
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RedeemedAt { get; set; }
        public string? RedeemedByUserId { get; set; }
        [ForeignKey("RedeemedByUserId")]
        public ApplicationUser? RedeemedByUser { get; set; }
    }
}
