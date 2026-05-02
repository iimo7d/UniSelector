namespace Uni_Selector.ViewModels.UserManagement
{
    public class UniversityRepDetailsData
    {
        public int RepresentativeId { get; set; }
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public string Position { get; set; }

        // University statistics
        public int UniversityProgramsCount { get; set; }
        public int PendingApplicationsCount { get; set; }
    }
}
