using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Hubs;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;

namespace Uni_Selector.Service.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            AppDbContext context,
            IHubContext<NotificationHub> hubContext,
            IEmailService emailService,
            UserManager<ApplicationUser> userManager,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SendNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationCategory category,
            NotificationChannel channel = NotificationChannel.InApp,
            string? actionUrl = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Category = category,
                    Channel = channel,
                    ActionUrl = actionUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                if (channel == NotificationChannel.InApp || channel == NotificationChannel.Email)
                {
                    await _hubContext.Clients.Group($"User_{userId}")
                        .SendAsync("ReceiveNotification", new
                        {
                            id = notification.Id,
                            title = notification.Title,
                            message = notification.Message,
                            category = notification.Category.ToString(),
                            actionUrl = notification.ActionUrl,
                            timestamp = notification.CreatedAt,
                            type = "info"
                        });
                }

                if (channel == NotificationChannel.Email)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await SendEmailNotificationAsync(user.Email, title, message, actionUrl);
                        notification.EmailSent = true;
                        notification.SentAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                _logger.LogInformation($"Notification sent to user {userId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to user {userId}");
                throw;
            }
        }

        public async Task SendNotificationToRoleAsync(
            string roleName,
            string title,
            string message,
            NotificationCategory category,
            NotificationChannel channel = NotificationChannel.InApp,
            string? actionUrl = null)
        {
            try
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

                foreach (var user in usersInRole)
                {
                    var notification = new Notification
                    {
                        UserId = user.Id,
                        Title = title,
                        Message = message,
                        Category = category,
                        Channel = channel,
                        ActionUrl = actionUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"Role_{roleName}")
                    .SendAsync("ReceiveNotification", new
                    {
                        title,
                        message,
                        category = category.ToString(),
                        actionUrl,
                        timestamp = DateTime.UtcNow,
                        type = "role"
                    });

                if (channel == NotificationChannel.Email)
                {
                    foreach (var user in usersInRole.Where(u => !string.IsNullOrEmpty(u.Email)))
                    {
                        try
                        {
                            await SendEmailNotificationAsync(user.Email!, title, message, actionUrl);

                            var notification = await _context.Notifications
                                .FirstOrDefaultAsync(n => n.UserId == user.Id && n.Title == title && !n.EmailSent);

                            if (notification != null)
                            {
                                notification.EmailSent = true;
                                notification.SentAt = DateTime.UtcNow;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to send email to user {user.Id}");
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Notification sent to role {roleName}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to role {roleName}");
                throw;
            }
        }

        public async Task SendBroadcastNotificationAsync(
            string title,
            string message,
            NotificationCategory category,
            NotificationChannel channel = NotificationChannel.InApp,
            string? actionUrl = null)
        {
            try
            {
                var allUsers = await _userManager.Users.Where(u => u.IsActive).ToListAsync();

                foreach (var user in allUsers)
                {
                    var notification = new Notification
                    {
                        UserId = user.Id,
                        Title = title,
                        Message = message,
                        Category = category,
                        Channel = channel,
                        ActionUrl = actionUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    category = category.ToString(),
                    actionUrl,
                    timestamp = DateTime.UtcNow,
                    type = "broadcast"
                });

                if (channel == NotificationChannel.Email)
                {
                    var emails = allUsers.Where(u => !string.IsNullOrEmpty(u.Email)).Select(u => u.Email!).ToList();
                    await _emailService.SendBulkEmailAsync(emails, title, message);

                    var notifications = await _context.Notifications
                        .Where(n => n.Title == title && !n.EmailSent)
                        .ToListAsync();

                    foreach (var notification in notifications)
                    {
                        notification.EmailSent = true;
                        notification.SentAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Broadcast notification sent: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send broadcast notification");
                throw;
            }
        }

        public async Task SendNotificationToUniversityAsync(
            int universityId,
            string title,
            string message,
            NotificationCategory category,
            NotificationChannel channel = NotificationChannel.InApp,
            string? actionUrl = null)
        {
            try
            {
                var universityReps = await _context.UniversityRepresentatives
                    .Where(ur => ur.UniversityId == universityId && ur.IsActive)
                    .Include(ur => ur.User)
                    .Select(ur => ur.User)
                    .ToListAsync();

                foreach (var user in universityReps)
                {
                    var notification = new Notification
                    {
                        UserId = user.Id,
                        Title = title,
                        Message = message,
                        Category = category,
                        Channel = channel,
                        ActionUrl = actionUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"University_{universityId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        title,
                        message,
                        category = category.ToString(),
                        actionUrl,
                        timestamp = DateTime.UtcNow,
                        type = "university"
                    });

                if (channel == NotificationChannel.Email)
                {
                    foreach (var user in universityReps.Where(u => !string.IsNullOrEmpty(u.Email)))
                    {
                        try
                        {
                            await SendEmailNotificationAsync(user.Email!, title, message, actionUrl);

                            var notification = await _context.Notifications
                                .FirstOrDefaultAsync(n => n.UserId == user.Id && n.Title == title && !n.EmailSent);

                            if (notification != null)
                            {
                                notification.EmailSent = true;
                                notification.SentAt = DateTime.UtcNow;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to send email to user {user.Id}");
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Notification sent to university {universityId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to university {universityId}");
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            try
            {
                return await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get unread count for user {userId}");
                return 0;
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get notifications for user {userId}");
                return new List<Notification>();
            }
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get unread notifications for user {userId}");
                return new List<Notification>();
            }
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId, string userId)
        {
            try
            {
                return await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get notification {notificationId}");
                return null;
            }
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification != null && !notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    var unreadCount = await GetUnreadCountAsync(userId);
                    await _hubContext.Clients.Group($"User_{userId}")
                        .SendAsync("UnreadCountUpdated", unreadCount);

                    _logger.LogInformation($"Notification {notificationId} marked as read");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark notification {notificationId} as read");
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("UnreadCountUpdated", 0);

                _logger.LogInformation($"All notifications marked as read for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark all notifications as read for user {userId}");
                throw;
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();

                    var unreadCount = await GetUnreadCountAsync(userId);
                    await _hubContext.Clients.Group($"User_{userId}")
                        .SendAsync("UnreadCountUpdated", unreadCount);

                    _logger.LogInformation($"Notification {notificationId} deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete notification {notificationId}");
                throw;
            }
        }

        public async Task DeleteOldNotificationsAsync(int daysOld = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

                var oldNotifications = await _context.Notifications
                    .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                    .ToListAsync();

                _context.Notifications.RemoveRange(oldNotifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted {oldNotifications.Count} old notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete old notifications");
                throw;
            }
        }

        private async Task SendEmailNotificationAsync(string email, string title, string message, string? actionUrl)
        {
            var emailBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f4f4f4; padding: 20px; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Smart University Platform</h1>
                        </div>
                        <div class='content'>
                            <h2>{title}</h2>
                            <p>{message}</p>
                            {(actionUrl != null ? $"<a href='{actionUrl}' class='button'>View Details</a>" : "")}
                        </div>
                        <div class='footer'>
                            <p>© 2024 Smart University Platform. All rights reserved.</p>
                            <p>This is an automated notification. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailService.SendEmailAsync(email, title, emailBody, isHtml: true);
        }
    }
}