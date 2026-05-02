namespace Uni_Selector.ViewModels.NotificationAdmin
{
    public class NotificationHistoryItemViewModel
    {
        public DateTime Date { get; set; }
        public int TotalSent { get; set; }
        public int TotalRead { get; set; }
        public decimal ReadRate => TotalSent > 0 ? (decimal)TotalRead / TotalSent * 100 : 0;
        public int EmailsSent { get; set; }
        public Dictionary<string, int> CategoriesBreakdown { get; set; } = new();
    }
}
