using Uni_Selector.Models;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Service.Interface
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, NotificationCategory category, NotificationChannel channel = NotificationChannel.InApp, string? actionUrl = null);

        Task SendNotificationToRoleAsync(string roleName, string title, string message, NotificationCategory category, NotificationChannel channel = NotificationChannel.InApp, string? actionUrl = null);

        Task SendBroadcastNotificationAsync(string title, string message, NotificationCategory category, NotificationChannel channel = NotificationChannel.InApp, string? actionUrl = null);

        Task SendNotificationToUniversityAsync(int universityId, string title, string message, NotificationCategory category, NotificationChannel channel = NotificationChannel.InApp, string? actionUrl = null);

        Task<int> GetUnreadCountAsync(string userId);

        Task<List<Notification>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 20);

        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);

        Task<Notification?> GetNotificationByIdAsync(int notificationId, string userId);

        Task MarkAsReadAsync(int notificationId, string userId);

        Task MarkAllAsReadAsync(string userId);

        Task DeleteNotificationAsync(int notificationId, string userId);

        Task DeleteOldNotificationsAsync(int daysOld = 90);
    }
}
