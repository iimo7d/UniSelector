using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;
using PathType = Uni_Selector.Models.Enums.PathType;

namespace Uni_Selector.Models
{
    public class EntryRequirement
    {
        public int Id { get; set; }
        [Required] public int UniversityProgramId { get; set; }
        [ForeignKey("UniversityProgramId")]
        public UniversityProgram UniversityProgram { get; set; }
        [Required, Range(0, 100)] public double MinGPA { get; set; }
        [Required] public PathType Path { get; set; }
        public AcademicTrack? AcademicTrack { get; set; }
        public VocationalBranch? VocationalBranch { get; set; }
        public bool AllowNoTawjihi { get; set; } = false;
        [StringLength(500)] public string? Notes { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
