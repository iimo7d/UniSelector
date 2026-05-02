using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;
using PathType = Uni_Selector.Models.Enums.PathType;

namespace Uni_Selector.Models
{
    public class Student
    {
        public int Id { get; set; }
        [Required] public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required, StringLength(100)] public string Province { get; set; }
        [Required, StringLength(100)] public string City { get; set; }
        [StringLength(200)] public string? Area { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Required, Range(0, 100)] public double GPA { get; set; }
        [Required] public PathType Path { get; set; }
        public AcademicTrack? AcademicTrack { get; set; }
        public VocationalBranch? VocationalBranch { get; set; }
        public bool BtecLevel2Completed { get; set; }
        public bool BtecLevel3Completed { get; set; }
        [StringLength(500)] public string? BtecCertificateUrl { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] public decimal RegistrationBudget { get; set; }
        [StringLength(500)] public string? DesiredMajors { get; set; }
        [StringLength(100)] public string? PreferredCity { get; set; }
        public int MaxDistanceKm { get; set; } = 50;
        public LanguageCode PreferredLanguage { get; set; } = LanguageCode.English;
        public bool HasFamilyConnection { get; set; }
        public int? FamilyConnectionUniversityId { get; set; }
        [ForeignKey("FamilyConnectionUniversityId")]
        public University? FamilyConnectionUniversity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string NationalId { get; set; }
        public string SeatNumber { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; } = "Jordanian";

        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianRelation { get; set; }

        public bool IsActive { get; set; } = true;


        public bool HasDisability { get; set; }
        public string? DisabilityType { get; set; }
        public bool IsOrphan { get; set; }
        public bool IsEmployeeChild { get; set; }

        public bool? ProfileCompleted { get; set; } = false;

        public ICollection<Recommendation> Recommendations { get; set; }
        public ICollection<StudentApplication> Applications { get; set; }
    }
}