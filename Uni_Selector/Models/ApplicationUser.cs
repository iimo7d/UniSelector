using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}

