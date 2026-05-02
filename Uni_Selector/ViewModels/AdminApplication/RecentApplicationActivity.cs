using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminApplication
{
    public class RecentApplicationActivity
    {
        public string ApplicationNumber { get; set; }
        public string StudentName { get; set; }
        public string UniversityName { get; set; }
        public ApplicationStatus Status { get; set; }
        public string StatusText { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; }
    }
}
