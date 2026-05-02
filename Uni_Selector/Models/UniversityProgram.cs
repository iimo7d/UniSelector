using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Models
{
    public class UniversityProgram
    {
        public int Id { get; set; }
        [Required] public int UniversityId { get; set; }
        [ForeignKey("UniversityId")]
        public University University { get; set; }
        [Required] public int ProgramId { get; set; }
        [ForeignKey("ProgramId")]
        public ProgramEntity Program { get; set; }
        [Required] public StudySystem StudySystem { get; set; }
        [Required, Range(1, 10)] public int DurationInYears { get; set; }
        [Required] public DateTime SemesterStartDate { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] public decimal HourPriceBase { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] public decimal RegistrationFeeFirstSemester { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] public decimal RegistrationFeeRegularSemester { get; set; }
        [Range(0, 10000)] public int Capacity { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<EntryRequirement> EntryRequirements { get; set; }
    }
}
