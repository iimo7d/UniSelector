namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class RepresentativeDetailDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? Position { get; set; }
        public bool CanManagePrograms { get; set; }
        public bool CanManageFees { get; set; }
        public bool CanViewApplications { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
