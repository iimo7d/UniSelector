using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Models
{
    public class StudentApplication
    {
        public int Id { get; set; }
        [Required] public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        public int? UniversityProgramId { get; set; }
        [ForeignKey("UniversityProgramId")]
        public UniversityProgram? UniversityProgram { get; set; }
        public int? BtecProgramId { get; set; }
        [ForeignKey("BtecProgramId")]
        public BtecProgram? BtecProgram { get; set; }
        [Required] public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovalDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
        [Range(30, 50)]
        public decimal? HourDiscountPercent { get; set; }
        public string? HourDiscountSetByUserId { get; set; }
        [ForeignKey("HourDiscountSetByUserId")]
        public ApplicationUser? HourDiscountSetByUser { get; set; }
        public DateTime? HourDiscountSetAt { get; set; }
        [Range(0, 60)] public int? PlannedFirstSemesterHours { get; set; }
        public DiscountGrant? DiscountGrant { get; set; }
        public Commission? Commission { get; set; }

        public string? ApplicationNumber { get; set; }  // رقم الطلب
        public string? AdmissionNumber { get; set; }  // رقم القبول
        public string? RejectionReason { get; set; }

    }
}
