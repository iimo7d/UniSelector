using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.NotificationAdmin
{
    public class NotificationTemplateItem
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationCategory Category { get; set; }
        public string CategoryText { get; set; }
        public string Description { get; set; }
    }
}
