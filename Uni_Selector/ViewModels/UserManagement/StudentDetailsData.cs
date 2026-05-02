using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.UserManagement
{
    public class StudentDetailsData
    {
        public int StudentId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public double GPA { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public PathType Path { get; set; }
        public decimal RegistrationBudget { get; set; }
        public bool ProfileCompleted { get; set; }

        // Application statistics
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
    }
}
