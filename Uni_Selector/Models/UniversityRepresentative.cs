using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uni_Selector.Models
{
    public class UniversityRepresentative
    {
        public int Id { get; set; }
        [Required] public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required] public int UniversityId { get; set; }
        [ForeignKey("UniversityId")]
        public University University { get; set; }
        [StringLength(100)] public string? Position { get; set; }
        public bool CanManagePrograms { get; set; } = true;
        public bool CanManageFees { get; set; } = true;
        public bool CanViewApplications { get; set; } = true;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}