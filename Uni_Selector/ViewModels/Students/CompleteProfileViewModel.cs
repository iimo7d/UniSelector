//using System.ComponentModel.DataAnnotations;
//using Uni_Selector.Models.Enums;

//namespace Uni_Selector.ViewModels.Students
//{
//    public class CompleteProfileViewModel
//    {
//        // Display Only Fields (from ApplicationUser)
//        public string FullName { get; set; }
//        public string Email { get; set; }
//        public string PhoneNumber { get; set; }

//        // Personal Information
//        [Required(ErrorMessage = "National ID is required")]
//        [StringLength(10, MinimumLength = 10, ErrorMessage = "National ID must be exactly 10 digits")]
//        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must contain only numbers")]
//        [Display(Name = "National ID")]
//        public string NationalId { get; set; }

//        [Required(ErrorMessage = "Seat Number is required")]
//        [StringLength(20, ErrorMessage = "Seat Number cannot exceed 20 characters")]
//        [Display(Name = "Seat Number (Tawjihi)")]
//        public string SeatNumber { get; set; }

//        [Required(ErrorMessage = "Gender is required")]
//        [Display(Name = "Gender")]
//        public Gender Gender { get; set; }

//        [Required(ErrorMessage = "Date of Birth is required")]
//        [DataType(DataType.Date)]
//        [Display(Name = "Date of Birth")]
//        public DateTime DateOfBirth { get; set; }

//        [StringLength(50)]
//        [Display(Name = "Nationality")]
//        public string? Nationality { get; set; } = "Jordanian";

//        // Guardian Information
//        [Required(ErrorMessage = "Guardian name is required")]
//        [StringLength(100, ErrorMessage = "Guardian name cannot exceed 100 characters")]
//        [Display(Name = "Guardian Full Name")]
//        public string GuardianName { get; set; }

//        [Required(ErrorMessage = "Guardian phone is required")]
//        [Phone(ErrorMessage = "Invalid phone number format")]
//        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
//        [Display(Name = "Guardian Phone Number")]
//        public string GuardianPhone { get; set; }

//        [Required(ErrorMessage = "Guardian relation is required")]
//        [StringLength(50, ErrorMessage = "Relation cannot exceed 50 characters")]
//        [Display(Name = "Guardian Relation")]
//        public string GuardianRelation { get; set; }

//        // Location Information
//        [Required(ErrorMessage = "Province is required")]
//        [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
//        [Display(Name = "Province")]
//        public string Province { get; set; }

//        [Required(ErrorMessage = "City is required")]
//        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
//        [Display(Name = "City")]
//        public string City { get; set; }

//        [StringLength(200, ErrorMessage = "Area cannot exceed 200 characters")]
//        [Display(Name = "Area/Neighborhood")]
//        public string? Area { get; set; }

//        // Academic Information
//        [Required(ErrorMessage = "GPA is required")]
//        [Range(0, 100, ErrorMessage = "GPA must be between 0 and 100")]
//        [Display(Name = "GPA (Tawjihi Score)")]
//        public double GPA { get; set; }

//        [Required(ErrorMessage = "Educational path is required")]
//        [Display(Name = "Educational Path")]
//        public PathType Path { get; set; }

//        [Display(Name = "Academic Track")]
//        public AcademicTrack? AcademicTrack { get; set; }

//        [Display(Name = "Vocational Branch")]
//        public VocationalBranch? VocationalBranch { get; set; }

//        [Display(Name = "BTEC Level 2 Completed")]
//        public bool BtecLevel2Completed { get; set; }

//        [Display(Name = "BTEC Level 3 Completed")]
//        public bool BtecLevel3Completed { get; set; }

//        [Display(Name = "BTEC Certificate Upload")]
//        public IFormFile? BtecCertificateFile { get; set; }

//        // Preferences
//        [Required(ErrorMessage = "Registration budget is required")]
//        [Range(0, 1000000, ErrorMessage = "Registration budget must be a positive value")]
//        [Display(Name = "Registration Budget (JOD)")]
//        public decimal RegistrationBudget { get; set; }

//        [StringLength(500, ErrorMessage = "Desired majors cannot exceed 500 characters")]
//        [Display(Name = "Desired Majors (comma separated)")]
//        public string? DesiredMajors { get; set; }

//        [StringLength(100, ErrorMessage = "Preferred city cannot exceed 100 characters")]
//        [Display(Name = "Preferred University City")]
//        public string? PreferredCity { get; set; }

//        [Range(1, 200, ErrorMessage = "Max distance must be between 1 and 200 km")]
//        [Display(Name = "Maximum Distance from University (km)")]
//        public int MaxDistanceKm { get; set; } = 50;

//        [Display(Name = "Preferred Language of Study")]
//        public LanguageCode PreferredLanguage { get; set; } = LanguageCode.English;

//        // Special Circumstances
//        [Display(Name = "Do you have a disability?")]
//        public bool HasDisability { get; set; }

//        [StringLength(200, ErrorMessage = "Disability type cannot exceed 200 characters")]
//        [Display(Name = "Disability Type (if applicable)")]
//        public string? DisabilityType { get; set; }

//        [Display(Name = "Are you an orphan?")]
//        public bool IsOrphan { get; set; }

//        [Display(Name = "Are you a university employee's child?")]
//        public bool IsEmployeeChild { get; set; }

//        [Display(Name = "Do you have family connection to a university?")]
//        public bool HasFamilyConnection { get; set; }

//        [Display(Name = "Family Connection University")]
//        public int? FamilyConnectionUniversityId { get; set; }
//    }
//}



// ================================
// File: ViewModels/Students/CompleteProfileViewModel.cs
// ================================
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.Students
{
    public class CompleteProfileViewModel : IValidatableObject
    {
        // Display only (from ApplicationUser)
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        // Personal Information
        [Required(ErrorMessage = "National ID is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "National ID must be exactly 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must contain only numbers")]
        [Display(Name = "National ID")]
        public string NationalId { get; set; } = "";

        [Required(ErrorMessage = "Seat Number is required")]
        [StringLength(20, ErrorMessage = "Seat Number cannot exceed 20 characters")]
        [Display(Name = "Seat Number (Tawjihi)")]
        public string SeatNumber { get; set; } = "";

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public Gender? Gender { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        [Display(Name = "Nationality")]
        public string? Nationality { get; set; } = "Jordanian";

        // Guardian Information
        [Required(ErrorMessage = "Guardian name is required")]
        [StringLength(100, ErrorMessage = "Guardian name cannot exceed 100 characters")]
        [Display(Name = "Guardian Full Name")]
        public string GuardianName { get; set; } = "";

        [Required(ErrorMessage = "Guardian phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Guardian Phone Number")]
        public string GuardianPhone { get; set; } = "";

        [Required(ErrorMessage = "Guardian relation is required")]
        [StringLength(50, ErrorMessage = "Relation cannot exceed 50 characters")]
        [Display(Name = "Guardian Relation")]
        public string GuardianRelation { get; set; } = "";

        // Location Information
        [Required(ErrorMessage = "Province is required")]
        [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
        [Display(Name = "Province")]
        public string Province { get; set; } = "";

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string City { get; set; } = "";

        [StringLength(200, ErrorMessage = "Area cannot exceed 200 characters")]
        [Display(Name = "Area/Neighborhood")]
        public string? Area { get; set; }

        // Academic Information
        [Required(ErrorMessage = "GPA is required")]
        [Range(0, 100, ErrorMessage = "GPA must be between 0 and 100")]
        [Display(Name = "GPA (Tawjihi Score)")]
        public double? GPA { get; set; }

        [Required(ErrorMessage = "Educational path is required")]
        [Display(Name = "Educational Path")]
        public PathType? Path { get; set; }

        [Display(Name = "Academic Track")]
        public AcademicTrack? AcademicTrack { get; set; }

        [Display(Name = "Vocational Branch")]
        public VocationalBranch? VocationalBranch { get; set; }

        [Display(Name = "BTEC Level 2 Completed")]
        public bool BtecLevel2Completed { get; set; }

        [Display(Name = "BTEC Level 3 Completed")]
        public bool BtecLevel3Completed { get; set; }

        [Display(Name = "BTEC Certificate Upload")]
        public IFormFile? BtecCertificateFile { get; set; }

        // Keeps existing certificate url when editing/resuming
        public string? ExistingBtecCertificateUrl { get; set; }

        // Preferences
        [Required(ErrorMessage = "Registration budget is required")]
        [Range(0, 1000000, ErrorMessage = "Registration budget must be a positive value")]
        [Display(Name = "Registration Budget (JOD)")]
        public decimal? RegistrationBudget { get; set; }

        [StringLength(500, ErrorMessage = "Desired majors cannot exceed 500 characters")]
        [Display(Name = "Desired Majors (comma separated)")]
        public string? DesiredMajors { get; set; }

        [StringLength(100, ErrorMessage = "Preferred city cannot exceed 100 characters")]
        [Display(Name = "Preferred University City")]
        public string? PreferredCity { get; set; }

        [Range(1, 200, ErrorMessage = "Max distance must be between 1 and 200 km")]
        [Display(Name = "Maximum Distance from University (km)")]
        public int MaxDistanceKm { get; set; } = 50;

        [Display(Name = "Preferred Language of Study")]
        public LanguageCode PreferredLanguage { get; set; } = LanguageCode.English;

        // Special Circumstances
        [Display(Name = "Do you have a disability?")]
        public bool HasDisability { get; set; }

        [StringLength(200, ErrorMessage = "Disability type cannot exceed 200 characters")]
        [Display(Name = "Disability Type (if applicable)")]
        public string? DisabilityType { get; set; }

        [Display(Name = "Are you an orphan?")]
        public bool IsOrphan { get; set; }

        [Display(Name = "Are you a university employee's child?")]
        public bool IsEmployeeChild { get; set; }

        [Display(Name = "Do you have family connection to a university?")]
        public bool HasFamilyConnection { get; set; }

        [Display(Name = "Family Connection University")]
        public int? FamilyConnectionUniversityId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // DateOfBirth must not be in the future
            if (DateOfBirth.HasValue && DateOfBirth.Value.Date > DateTime.UtcNow.Date)
                yield return new ValidationResult("Date of Birth cannot be in the future.", new[] { nameof(DateOfBirth) });

            // Conditional fields by Path
            if (Path == PathType.Academic && AcademicTrack == null)
                yield return new ValidationResult("Academic Track is required for Academic path.", new[] { nameof(AcademicTrack) });

            if (Path == PathType.Vocational && VocationalBranch == null)
                yield return new ValidationResult("Vocational Branch is required for Vocational path.", new[] { nameof(VocationalBranch) });

            if (Path == PathType.BTEC)
            {
                // Certificate required if none exists
                if (string.IsNullOrWhiteSpace(ExistingBtecCertificateUrl) && BtecCertificateFile == null)
                    yield return new ValidationResult("BTEC certificate is required for BTEC path.", new[] { nameof(BtecCertificateFile) });
            }

            // Disability type required if checked
            if (HasDisability && string.IsNullOrWhiteSpace(DisabilityType))
                yield return new ValidationResult("Please specify disability type.", new[] { nameof(DisabilityType) });

            // Family connection university required if checked
            if (HasFamilyConnection && FamilyConnectionUniversityId == null)
                yield return new ValidationResult("Please select the university for family connection.", new[] { nameof(FamilyConnectionUniversityId) });
        }
    }
}
