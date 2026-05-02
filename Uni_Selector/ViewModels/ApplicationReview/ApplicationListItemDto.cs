using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ApplicationReview
{
    public class ApplicationListItemDto
    {
        public int Id { get; set; }
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string ProgramName { get; set; }
        public bool IsBtecProgram { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public double StudentGPA { get; set; }
        public PathType StudentPath { get; set; }
    }
}
