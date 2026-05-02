using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Uni_Selector.Service.Interface;

namespace Uni_Selector.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get unread notification count - Used in layout notification badge
        /// GET /api/notifications/count
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return StatusCode(500, new { success = false, message = "Failed to get notification count" });
            }
        }

        /// <summary>
        /// Get recent notifications - Used in notification dropdown
        /// GET /api/notifications/recent?limit=10
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentNotifications([FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, 1, limit);

                var result = notifications.Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    Category = n.Category.ToString(),
                    n.IsRead,
                    n.ActionUrl,
                    CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                });

                return Ok(new { success = true, notifications = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications");
                return StatusCode(500, new { success = false, message = "Failed to get notifications" });
            }
        }

        /// <summary>
        /// Get paginated notifications - Used in notifications page
        /// GET /api/notifications?page=1&pageSize=20
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);

                var result = notifications.Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    Category = n.Category.ToString(),
                    n.IsRead,
                    n.ActionUrl,
                    CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                });

                return Ok(new
                {
                    success = true,
                    notifications = result,
                    page,
                    pageSize,
                    hasMore = notifications.Count == pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { success = false, message = "Failed to get notifications" });
            }
        }

        /// <summary>
        /// Get unread notifications only
        /// GET /api/notifications/unread
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);

                var result = notifications.Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    Category = n.Category.ToString(),
                    n.ActionUrl,
                    CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                });

                return Ok(new { success = true, notifications = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return StatusCode(500, new { success = false, message = "Failed to get notifications" });
            }
        }

        /// <summary>
        /// Mark single notification as read
        /// POST /api/notifications/{id}/mark-read
        /// </summary>
        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                await _notificationService.MarkAsReadAsync(id, userId);
                var newCount = await _notificationService.GetUnreadCountAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Notification marked as read",
                    unreadCount = newCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {id} as read");
                return StatusCode(500, new { success = false, message = "Failed to mark notification as read" });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// POST /api/notifications/mark-all-read
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                await _notificationService.MarkAllAsReadAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "All notifications marked as read",
                    unreadCount = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { success = false, message = "Failed to mark all notifications as read" });
            }
        }

        /// <summary>
        /// Delete a notification
        /// DELETE /api/notifications/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                await _notificationService.DeleteNotificationAsync(id, userId);
                var newCount = await _notificationService.GetUnreadCountAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Notification deleted",
                    unreadCount = newCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting notification {id}");
                return StatusCode(500, new { success = false, message = "Failed to delete notification" });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// GET /api/notifications/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var notification = await _notificationService.GetNotificationByIdAsync(id, userId);

                if (notification == null)
                    return NotFound(new { success = false, message = "Notification not found" });

                var result = new
                {
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    Category = notification.Category.ToString(),
                    notification.IsRead,
                    notification.ActionUrl,
                    CreatedAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeAgo = GetTimeAgo(notification.CreatedAt)
                };

                return Ok(new { success = true, notification = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting notification {id}");
                return StatusCode(500, new { success = false, message = "Failed to get notification" });
            }
        }

        #region Helper Methods

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) != 1 ? "s" : "")} ago";
        }

        #endregion
    }
}
