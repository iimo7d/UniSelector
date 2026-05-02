namespace Uni_Selector.ViewModels.Account
{
    public class UserProfileViewModel
    {
        // Common fields for all users
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string Role { get; set; }

        // Student-specific fields
        public string? NationalId { get; set; }
        public string? SeatNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public double? GPA { get; set; }
        public string? PathType { get; set; }
        public string? AcademicTrack { get; set; }
        public string? VocationalBranch { get; set; }
        public bool? BtecLevel2Completed { get; set; }
        public bool? BtecLevel3Completed { get; set; }
        public string? BtecCertificateUrl { get; set; }
        public decimal? RegistrationBudget { get; set; }
        public string? DesiredMajors { get; set; }
        public string? PreferredCity { get; set; }
        public int? MaxDistanceKm { get; set; }
        public string? PreferredLanguage { get; set; }
        public bool? HasFamilyConnection { get; set; }
        public string? FamilyConnectionUniversityName { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }
        public string? GuardianRelation { get; set; }
        public bool? HasDisability { get; set; }
        public string? DisabilityType { get; set; }
        public bool? IsOrphan { get; set; }
        public bool? IsEmployeeChild { get; set; }
        public bool? ProfileCompleted { get; set; }

        // University Representative-specific fields
        public string? UniversityName { get; set; }
        public string? Position { get; set; }
        public bool? CanManagePrograms { get; set; }
        public bool? CanManageFees { get; set; }
        public bool? CanViewApplications { get; set; }
        public DateTime? AssignedAt { get; set; }
        public bool? RepIsActive { get; set; }
    }
}
