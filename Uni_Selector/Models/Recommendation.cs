using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uni_Selector.Models
{
    public class Recommendation
    {
        public int Id { get; set; }
        [Required] public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        public int? UniversityProgramId { get; set; }
        [ForeignKey("UniversityProgramId")]
        public UniversityProgram? UniversityProgram { get; set; }
        public int? BtecProgramId { get; set; }
        [ForeignKey("BtecProgramId")]
        public BtecProgram? BtecProgram { get; set; }
        [Required, Range(0, 100)] public double Score { get; set; }
        [StringLength(1000)] public string? ReasonsJson { get; set; }
        public double? DistanceInKm { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal EstimatedTotalCost { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsViewed { get; set; } = false;
    }
}
