using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.NotificationAdmin
{
    public class NotificationListItemViewModel
    {
        public int Id { get; set; }
        public string RecipientName { get; set; }
        public string RecipientEmail { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationCategory Category { get; set; }
        public string CategoryText { get; set; }
        public NotificationChannel Channel { get; set; }
        public string ChannelText { get; set; }
        public bool IsRead { get; set; }
        public bool EmailSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ActionUrl { get; set; }
    }
}
