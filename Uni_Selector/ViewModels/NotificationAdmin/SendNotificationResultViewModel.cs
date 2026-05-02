namespace Uni_Selector.ViewModels.NotificationAdmin
{
    public class SendNotificationResultViewModel
    {
        public bool Success { get; set; }
        public int TotalSent { get; set; }
        public int EmailsSent { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
