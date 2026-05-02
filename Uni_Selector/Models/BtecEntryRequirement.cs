using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uni_Selector.Models
{
    public class BtecEntryRequirement
    {
        public int Id { get; set; }
        [Required]
        public int BtecProgramId { get; set; }
        [ForeignKey("BtecProgramId")]
        public BtecProgram BtecProgram { get; set; }
        [Required, Range(0, 100)]
        public double MinGPA { get; set; }
        public bool RequiresBtecL2 { get; set; } = true;
        public bool RequiresBtecL3 { get; set; } = true;
        [StringLength(500)]
        public string? Notes { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}

