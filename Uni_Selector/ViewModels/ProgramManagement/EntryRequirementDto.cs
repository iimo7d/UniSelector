using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class EntryRequirementDto
    {
        public int Id { get; set; }
        public double MinGPA { get; set; }
        public PathType Path { get; set; }
        public AcademicTrack? AcademicTrack { get; set; }
        public VocationalBranch? VocationalBranch { get; set; }
        public bool AllowNoTawjihi { get; set; }
        public string? Notes { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
