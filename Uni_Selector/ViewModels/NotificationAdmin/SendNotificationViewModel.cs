using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.NotificationAdmin
{
    public class SendNotificationViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public NotificationCategory Category { get; set; }

        [Required(ErrorMessage = "Channel is required")]
        [Display(Name = "Channel")]
        public NotificationChannel Channel { get; set; }

        [Display(Name = "Action URL")]
        [StringLength(500, ErrorMessage = "Action URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string? ActionUrl { get; set; }

        [Required(ErrorMessage = "Please select recipient type")]
        [Display(Name = "Send To")]
        public string RecipientType { get; set; } // String, not enum: All, Students, UniversityReps, Role, Specific

        [Display(Name = "Specific Role")]
        public string? SpecificRole { get; set; }

        [Display(Name = "Specific User IDs")]
        public string? SpecificUserIds { get; set; } // Comma-separated user IDs

        // Available options
        public List<string> AvailableRoles { get; set; } = new();
        public List<RecipientTypeOption> RecipientTypes { get; set; } = new();
    }
}
