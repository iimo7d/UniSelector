using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(AppDbContext context, ILogger<NotificationHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User connected without valid identifier");
                await base.OnConnectedAsync();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            _logger.LogInformation($"User {userId} connected with connection {Context.ConnectionId}");

            var roles = Context.User?.Claims
                .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value)
                .ToList();

            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
                    _logger.LogInformation($"Added user {userId} to role group {role}");
                }
            }

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation($"User {userId} disconnected from connection {Context.ConnectionId}");
            }

            if (exception != null)
            {
                _logger.LogError(exception, $"User {userId} disconnected with error");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string userId, string title, string message, string? actionUrl = null)
        {
            try
            {
                await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    actionUrl,
                    timestamp = DateTime.UtcNow,
                    type = "info"
                });

                _logger.LogInformation($"Notification sent to user {userId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to user {userId}");
            }
        }

        public async Task SendNotificationToRole(string roleName, string title, string message, string? actionUrl = null)
        {
            try
            {
                await Clients.Group($"Role_{roleName}").SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    actionUrl,
                    timestamp = DateTime.UtcNow,
                    type = "role"
                });

                _logger.LogInformation($"Notification sent to role {roleName}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to role {roleName}");
            }
        }

        public async Task SendNotificationToAll(string title, string message, string? actionUrl = null)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    actionUrl,
                    timestamp = DateTime.UtcNow,
                    type = "broadcast"
                });

                _logger.LogInformation($"Broadcast notification sent: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send broadcast notification");
            }
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);

                    var unreadCount = await _context.Notifications
                        .CountAsync(n => n.UserId == userId && !n.IsRead);

                    await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);

                    _logger.LogInformation($"Notification {notificationId} marked as read by user {userId}");
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Notification not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark notification {notificationId} as read");
                await Clients.Caller.SendAsync("Error", "Failed to mark notification as read");
            }
        }

        public async Task MarkAllNotificationsAsRead()
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                await Clients.Caller.SendAsync("AllNotificationsMarkedAsRead");
                await Clients.Caller.SendAsync("UnreadCountUpdated", 0);

                _logger.LogInformation($"All notifications marked as read for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark all notifications as read");
                await Clients.Caller.SendAsync("Error", "Failed to mark all notifications as read");
            }
        }

        public async Task GetUnreadCount()
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("UnreadCountUpdated", 0);
                    return;
                }

                var count = await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                await Clients.Caller.SendAsync("UnreadCountUpdated", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread count");
                await Clients.Caller.SendAsync("UnreadCountUpdated", 0);
            }
        }

        public async Task GetRecentNotifications(int count = 10)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("RecentNotifications", new List<object>());
                    return;
                }

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(count)
                    .Select(n => new
                    {
                        n.Id,
                        n.Title,
                        n.Message,
                        n.Category,
                        n.IsRead,
                        n.CreatedAt,
                        n.ActionUrl
                    })
                    .ToListAsync();

                await Clients.Caller.SendAsync("RecentNotifications", notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent notifications");
                await Clients.Caller.SendAsync("RecentNotifications", new List<object>());
            }
        }

        public async Task JoinUniversityGroup(int universityId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"University_{universityId}");
                _logger.LogInformation($"Connection {Context.ConnectionId} joined university group {universityId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to join university group {universityId}");
            }
        }

        public async Task LeaveUniversityGroup(int universityId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"University_{universityId}");
                _logger.LogInformation($"Connection {Context.ConnectionId} left university group {universityId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to leave university group {universityId}");
            }
        }

        public async Task SendNotificationToUniversity(int universityId, string title, string message, string? actionUrl = null)
        {
            try
            {
                await Clients.Group($"University_{universityId}").SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    actionUrl,
                    timestamp = DateTime.UtcNow,
                    type = "university"
                });

                _logger.LogInformation($"Notification sent to university {universityId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to university {universityId}");
            }
        }
    }
}

