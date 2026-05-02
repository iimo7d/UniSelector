using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Models
{
    public class Notification
    {
        public int Id { get; set; }
        [Required] public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required, StringLength(200)] public string Title { get; set; }
        [Required, StringLength(1000)] public string Message { get; set; }
        [Required] public NotificationCategory Category { get; set; }
        [Required] public NotificationChannel Channel { get; set; }
        public bool IsRead { get; set; } = false;
        public bool EmailSent { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public DateTime? SentAt { get; set; }
        [StringLength(500)] public string? ActionUrl { get; set; }
    }
}
