using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Models
{
    public class BtecProgram
    {
        public int Id { get; set; }
        [Required] public int UniversityId { get; set; }
        [ForeignKey("UniversityId")]
        public University University { get; set; }
        [Required, StringLength(200)] 
        public string NameArabic { get; set; }
        [Required, StringLength(200)]
        public string NameEnglish { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
        [Required] 
        public BtecLevel Level { get; set; }
        [Required, StringLength(100)]
        public string TechnicalField { get; set; }
        [Required] 
        public LanguageCode Language { get; set; }
        [Required, Range(1, 10)]
        public int DurationInYears { get; set; }
        [Required]
        public DateTime SemesterStartDate { get; set; }
        [Required, Range(30, 300)] 
        public int TotalCreditHours { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] 
        public decimal HourPriceBase { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] 
        public decimal RegistrationFeeFirstSemester { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")] 
        public decimal RegistrationFeeRegularSemester { get; set; }
        [Range(0, 1000)] public int Capacity { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsApprovedByBtecAuthority { get; set; } = false;
        public DateTime? ApprovalDate { get; set; }
        [StringLength(500)]
        public string? ApprovalNotes { get; set; }
        [StringLength(500)]
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<BtecEntryRequirement> EntryRequirements { get; set; }
    }
}
