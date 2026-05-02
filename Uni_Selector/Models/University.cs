using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Models
{
    public class University
    {
        public int Id { get; set; }

        [Required, StringLength(200)] 
        public string NameArabic { get; set; }
        [Required, StringLength(200)] 
        public string NameEnglish { get; set; }
        [Required] 
        public UniversityType Type { get; set; } = UniversityType.Private;
        [Required, StringLength(100)] 
        public string Province { get; set; }
        [Required, StringLength(100)] 
        public string City { get; set; }
        [Required, StringLength(500)] 
        public string FullAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [StringLength(200)] 
        public string? AcademicAccreditation { get; set; }
        [Required, Phone, StringLength(20)] 
        public string PhoneNumber { get; set; }
        [Required, EmailAddress, StringLength(100)] 
        public string Email { get; set; }
        [Url, StringLength(500)]
        public string? OfficialWebsite { get; set; }
        [StringLength(500)] 
        public string? LogoPath { get; set; }
        [StringLength(500)] 
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public CommissionMode CommissionMode { get; set; } = CommissionMode.FirstSemesterRegistration2Percent;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<UniversityProgram> UniversityPrograms { get; set; }
        public ICollection<BtecProgram> BtecPrograms { get; set; }
        public ICollection<UniversityRepresentative> Representatives { get; set; }
    }
}
