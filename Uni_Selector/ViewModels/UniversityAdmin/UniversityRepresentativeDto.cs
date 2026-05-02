namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class UniversityRepresentativeDto
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? Position { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
