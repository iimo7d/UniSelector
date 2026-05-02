namespace Uni_Selector.ViewModels.NotificationAdmin
{
    public class NotificationHistoryViewModel
    {
        public List<NotificationHistoryItemViewModel> History { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        // Date filter
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Statistics by time period
        public Dictionary<string, int> DailyStats { get; set; } = new(); // Date -> Count
        public Dictionary<string, int> CategoryStats { get; set; } = new(); // Category -> Count
        public Dictionary<string, int> ChannelStats { get; set; } = new(); // Channel -> Count

        // Summary
        public int TotalSentThisMonth { get; set; }
        public int TotalReadThisMonth { get; set; }
        public decimal ReadRateThisMonth { get; set; }
        public int EmailsSentThisMonth { get; set; }
    }
}
