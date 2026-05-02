using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.UniversityReps
{
    public class RecentApplicationViewModel
    {
        public int Id { get; set; }
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string ProgramName { get; set; }
        public string ProgramType { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
    }
}
