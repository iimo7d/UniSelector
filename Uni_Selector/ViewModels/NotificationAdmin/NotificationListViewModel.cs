using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.NotificationAdmin
{
    public class NotificationListViewModel
    {
        public List<NotificationListItemViewModel> Notifications { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalNotifications { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalNotifications / PageSize);

        // Filters
        public string? SearchTerm { get; set; }
        public NotificationCategory? Category { get; set; }
        public NotificationChannel? Channel { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Summary
        public int TotalSent { get; set; }
        public int TotalRead { get; set; }
        public int TotalUnread { get; set; }
        public int EmailsSent { get; set; }

        // Filter options
        public List<NotificationCategory> AllCategories { get; set; } = new();
        public List<NotificationChannel> AllChannels { get; set; } = new();
    }
}
