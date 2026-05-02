using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uni_Selector.Models
{
    public class MonthlySettlement
    {
        public int Id { get; set; }
        [Required] public int UniversityId { get; set; }
        [ForeignKey("UniversityId")]
        public University University { get; set; }
        [Required, Range(2020, 2100)] public int Year { get; set; }
        [Required, Range(1, 12)] public int Month { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] public decimal TotalCommission { get; set; }
        [Required] public int StudentCount { get; set; }
        public bool Closed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public string? ClosedByUserId { get; set; }
        [ForeignKey("ClosedByUserId")]
        public ApplicationUser? ClosedByUser { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
        public ICollection<Commission> Commissions { get; set; }
    }
}
