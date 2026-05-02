using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.NotificationAdmin;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.PlatformAdmin)]
    public class NotificationAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationAdminController> _logger;

        public NotificationAdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<NotificationAdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            NotificationCategory? category = null,
            NotificationChannel? channel = null,
            bool? isRead = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(n =>
                        n.Title.Contains(searchTerm) ||
                        n.Message.Contains(searchTerm) ||
                        n.User.FullName.Contains(searchTerm) ||
                        n.User.Email.Contains(searchTerm));
                }

                if (category.HasValue)
                {
                    query = query.Where(n => n.Category == category.Value);
                }

                if (channel.HasValue)
                {
                    query = query.Where(n => n.Channel == channel.Value);
                }

                if (isRead.HasValue)
                {
                    query = query.Where(n => n.IsRead == isRead.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt <= endDate.Value.AddDays(1));
                }

                var totalNotifications = await query.CountAsync();

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new NotificationListItemViewModel
                    {
                        Id = n.Id,
                        RecipientName = n.User.FullName,
                        RecipientEmail = n.User.Email,
                        Title = n.Title,
                        Message = n.Message,
                        Category = n.Category,
                        CategoryText = n.Category.ToString(),
                        Channel = n.Channel,
                        ChannelText = n.Channel.ToString(),
                        IsRead = n.IsRead,
                        EmailSent = n.EmailSent,
                        CreatedAt = n.CreatedAt,
                        ReadAt = n.ReadAt,
                        SentAt = n.SentAt,
                        ActionUrl = n.ActionUrl
                    })
                    .ToListAsync();

                // Calculate summary
                var allNotifications = await _context.Notifications.ToListAsync();
                var totalSent = allNotifications.Count;
                var totalRead = allNotifications.Count(n => n.IsRead);
                var totalUnread = allNotifications.Count(n => !n.IsRead);
                var emailsSent = allNotifications.Count(n => n.EmailSent);

                var viewModel = new NotificationListViewModel
                {
                    Notifications = notifications,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalNotifications = totalNotifications,
                    SearchTerm = searchTerm,
                    Category = category,
                    Channel = channel,
                    IsRead = isRead,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalSent = totalSent,
                    TotalRead = totalRead,
                    TotalUnread = totalUnread,
                    EmailsSent = emailsSent,
                    AllCategories = Enum.GetValues(typeof(NotificationCategory)).Cast<NotificationCategory>().ToList(),
                    AllChannels = Enum.GetValues(typeof(NotificationChannel)).Cast<NotificationChannel>().ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications list");
                TempData["Error"] = "An error occurred while loading notifications.";
                return View(new NotificationListViewModel());
            }
        }

        public IActionResult Send()
        {
            try
            {
                var viewModel = new SendNotificationViewModel
                {
                    AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" },
                    RecipientTypes = new List<RecipientTypeOption>
                    {
                        new RecipientTypeOption { Value = "All", Text = "All Users", Description = "Send to all registered users" },
                        new RecipientTypeOption { Value = "Students", Text = "All Students", Description = "Send to all students" },
                        new RecipientTypeOption { Value = "UniversityReps", Text = "All University Representatives", Description = "Send to all university representatives" },
                        new RecipientTypeOption { Value = "Role", Text = "Specific Role", Description = "Send to users with a specific role" },
                        new RecipientTypeOption { Value = "Specific", Text = "Specific Users", Description = "Send to specific user IDs" }
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading send notification form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendNotificationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                    model.RecipientTypes = new List<RecipientTypeOption>
                    {
                        new RecipientTypeOption { Value = "All", Text = "All Users", Description = "Send to all registered users" },
                        new RecipientTypeOption { Value = "Students", Text = "All Students", Description = "Send to all students" },
                        new RecipientTypeOption { Value = "UniversityReps", Text = "All University Representatives", Description = "Send to all university representatives" },
                        new RecipientTypeOption { Value = "Role", Text = "Specific Role", Description = "Send to users with a specific role" },
                        new RecipientTypeOption { Value = "Specific", Text = "Specific Users", Description = "Send to specific user IDs" }
                    };
                    return View(model);
                }

                int totalSent = 0;
                int emailsSent = 0;

                // Send notifications using the INotificationService
                switch (model.RecipientType)
                {
                    case "All":
                        await _notificationService.SendBroadcastNotificationAsync(
                            model.Title,
                            model.Message,
                            model.Category,
                            model.Channel,
                            model.ActionUrl);

                        totalSent = await _userManager.Users.Where(u => u.IsActive).CountAsync();
                        if (model.Channel == NotificationChannel.Email)
                            emailsSent = totalSent;
                        break;

                    case "Students":
                        await _notificationService.SendNotificationToRoleAsync(
                            "Student",
                            model.Title,
                            model.Message,
                            model.Category,
                            model.Channel,
                            model.ActionUrl);

                        totalSent = (await _userManager.GetUsersInRoleAsync("Student")).Count;
                        if (model.Channel == NotificationChannel.Email)
                            emailsSent = totalSent;
                        break;

                    case "UniversityReps":
                        await _notificationService.SendNotificationToRoleAsync(
                            "UniversityRepresentative",
                            model.Title,
                            model.Message,
                            model.Category,
                            model.Channel,
                            model.ActionUrl);

                        totalSent = (await _userManager.GetUsersInRoleAsync("UniversityRepresentative")).Count;
                        if (model.Channel == NotificationChannel.Email)
                            emailsSent = totalSent;
                        break;

                    case "Role":
                        if (string.IsNullOrWhiteSpace(model.SpecificRole))
                        {
                            ModelState.AddModelError("SpecificRole", "Please select a role.");
                            model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                            model.RecipientTypes = new List<RecipientTypeOption>
                            {
                                new RecipientTypeOption { Value = "All", Text = "All Users", Description = "Send to all registered users" },
                                new RecipientTypeOption { Value = "Students", Text = "All Students", Description = "Send to all students" },
                                new RecipientTypeOption { Value = "UniversityReps", Text = "All University Representatives", Description = "Send to all university representatives" },
                                new RecipientTypeOption { Value = "Role", Text = "Specific Role", Description = "Send to users with a specific role" },
                                new RecipientTypeOption { Value = "Specific", Text = "Specific Users", Description = "Send to specific user IDs" }
                            };
                            return View(model);
                        }

                        await _notificationService.SendNotificationToRoleAsync(
                            model.SpecificRole,
                            model.Title,
                            model.Message,
                            model.Category,
                            model.Channel,
                            model.ActionUrl);

                        totalSent = (await _userManager.GetUsersInRoleAsync(model.SpecificRole)).Count;
                        if (model.Channel == NotificationChannel.Email)
                            emailsSent = totalSent;
                        break;

                    case "Specific":
                        if (string.IsNullOrWhiteSpace(model.SpecificUserIds))
                        {
                            ModelState.AddModelError("SpecificUserIds", "Please enter user IDs.");
                            model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                            model.RecipientTypes = new List<RecipientTypeOption>
                            {
                                new RecipientTypeOption { Value = "All", Text = "All Users", Description = "Send to all registered users" },
                                new RecipientTypeOption { Value = "Students", Text = "All Students", Description = "Send to all students" },
                                new RecipientTypeOption { Value = "UniversityReps", Text = "All University Representatives", Description = "Send to all university representatives" },
                                new RecipientTypeOption { Value = "Role", Text = "Specific Role", Description = "Send to users with a specific role" },
                                new RecipientTypeOption { Value = "Specific", Text = "Specific Users", Description = "Send to specific user IDs" }
                            };
                            return View(model);
                        }

                        var userIds = model.SpecificUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => id.Trim())
                            .ToList();

                        var recipients = await _userManager.Users
                            .Where(u => userIds.Contains(u.Id) && u.IsActive)
                            .ToListAsync();

                        if (!recipients.Any())
                        {
                            TempData["Warning"] = "No active recipients found for the specified user IDs.";
                            return RedirectToAction(nameof(Send));
                        }

                        foreach (var user in recipients)
                        {
                            await _notificationService.SendNotificationAsync(
                                user.Id,
                                model.Title,
                                model.Message,
                                model.Category,
                                model.Channel,
                                model.ActionUrl);
                        }

                        totalSent = recipients.Count;
                        if (model.Channel == NotificationChannel.Email)
                            emailsSent = totalSent;
                        break;
                }

                if (totalSent == 0)
                {
                    TempData["Warning"] = "No active recipients found for the selected criteria.";
                    return RedirectToAction(nameof(Send));
                }

                _logger.LogInformation($"Bulk notification sent to {totalSent} users. Category: {model.Category}, Channel: {model.Channel}");

                var successMessage = $"Successfully sent notification to {totalSent} user(s).";
                if (emailsSent > 0)
                    successMessage += $" Emails sent: {emailsSent}";

                TempData["Success"] = successMessage;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                ModelState.AddModelError("", "An error occurred while sending the notification.");
                model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                model.RecipientTypes = new List<RecipientTypeOption>
                {
                    new RecipientTypeOption { Value = "All", Text = "All Users", Description = "Send to all registered users" },
                    new RecipientTypeOption { Value = "Students", Text = "All Students", Description = "Send to all students" },
                    new RecipientTypeOption { Value = "UniversityReps", Text = "All University Representatives", Description = "Send to all university representatives" },
                    new RecipientTypeOption { Value = "Role", Text = "Specific Role", Description = "Send to users with a specific role" },
                    new RecipientTypeOption { Value = "Specific", Text = "Specific Users", Description = "Send to specific user IDs" }
                };
                return View(model);
            }
        }

        public IActionResult Templates()
        {
            try
            {
                var templates = new List<NotificationTemplateItem>
                {
                    new NotificationTemplateItem
                    {
                        Name = "New Recommendation",
                        Title = "New University Recommendation",
                        Message = "We've found a new university program that matches your profile. Check it out now!",
                        Category = NotificationCategory.NewRecommendation,
                        CategoryText = "NewRecommendation",
                        Description = "Sent when a new recommendation is generated for a student"
                    },
                    new NotificationTemplateItem
                    {
                        Name = "Application Submitted",
                        Title = "Application Submitted Successfully",
                        Message = "Your application has been submitted and is now under review by the university.",
                        Category = NotificationCategory.ApplicationSubmitted,
                        CategoryText = "ApplicationSubmitted",
                        Description = "Sent when a student submits an application"
                    },
                    new NotificationTemplateItem
                    {
                        Name = "Application Approved",
                        Title = "Congratulations! Application Approved",
                        Message = "Your application has been approved! Please check your email for next steps.",
                        Category = NotificationCategory.ApplicationApproved,
                        CategoryText = "ApplicationApproved",
                        Description = "Sent when a university approves an application"
                    },
                    new NotificationTemplateItem
                    {
                        Name = "Application Rejected",
                        Title = "Application Update",
                        Message = "We regret to inform you that your application was not approved at this time.",
                        Category = NotificationCategory.ApplicationRejected,
                        CategoryText = "ApplicationRejected",
                        Description = "Sent when a university rejects an application"
                    },
                    new NotificationTemplateItem
                    {
                        Name = "Discount Code Generated",
                        Title = "Your Discount Code is Ready!",
                        Message = "Great news! Your 5% discount code has been generated and is ready to use.",
                        Category = NotificationCategory.PromoCodeGenerated,
                        CategoryText = "PromoCodeGenerated",
                        Description = "Sent when a discount code is generated for a student"
                    },
                    new NotificationTemplateItem
                    {
                        Name = "System Alert",
                        Title = "System Notification",
                        Message = "Important system update or announcement.",
                        Category = NotificationCategory.SystemAlert,
                        CategoryText = "SystemAlert",
                        Description = "Sent for important system announcements"
                    },
                    new NotificationTemplateItem
                    {
                        Name = "Commission Generated",
                        Title = "Commission Calculated",
                        Message = "A new commission has been calculated for your university.",
                        Category = NotificationCategory.CommissionGenerated,
                        CategoryText = "CommissionGenerated",
                        Description = "Sent to universities when a commission is calculated"
                    },
                    new NotificationTemplateItem
                    {
                        Name = "Discount Applied",
                        Title = "Discount Successfully Applied",
                        Message = "Your discount has been successfully applied to your application.",
                        Category = NotificationCategory.DiscountApplied,
                        CategoryText = "DiscountApplied",
                        Description = "Sent when a discount is applied to an application"
                    }
                };

                var viewModel = new NotificationTemplatesViewModel
                {
                    Templates = templates
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification templates");
                TempData["Error"] = "An error occurred while loading templates.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> History(
            int page = 1,
            int pageSize = 10,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                // Default to last 30 days if no dates provided
                var end = endDate ?? DateTime.UtcNow.Date;
                var start = startDate ?? end.AddDays(-30);

                var notifications = await _context.Notifications
                    .Where(n => n.CreatedAt >= start && n.CreatedAt <= end.AddDays(1))
                    .ToListAsync();

                // Group by date
                var dailyStats = notifications
                    .GroupBy(n => n.CreatedAt.Date)
                    .OrderByDescending(g => g.Key)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new NotificationHistoryItemViewModel
                    {
                        Date = g.Key,
                        TotalSent = g.Count(),
                        TotalRead = g.Count(n => n.IsRead),
                        EmailsSent = g.Count(n => n.EmailSent),
                        CategoriesBreakdown = g.GroupBy(n => n.Category.ToString())
                            .ToDictionary(cg => cg.Key, cg => cg.Count())
                    })
                    .ToList();

                var totalDays = notifications.GroupBy(n => n.CreatedAt.Date).Count();

                // Calculate monthly stats
                var thisMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var thisMonthNotifications = await _context.Notifications
                    .Where(n => n.CreatedAt >= thisMonthStart)
                    .ToListAsync();

                var totalSentThisMonth = thisMonthNotifications.Count;
                var totalReadThisMonth = thisMonthNotifications.Count(n => n.IsRead);
                var readRateThisMonth = totalSentThisMonth > 0 ? (decimal)totalReadThisMonth / totalSentThisMonth * 100 : 0;

                // Category stats for the period
                var categoryStats = notifications
                    .GroupBy(n => n.Category.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                // Channel stats for the period
                var channelStats = notifications
                    .GroupBy(n => n.Channel.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                var viewModel = new NotificationHistoryViewModel
                {
                    History = dailyStats,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalDays,
                    StartDate = start,
                    EndDate = end,
                    DailyStats = notifications.GroupBy(n => n.CreatedAt.Date.ToString("yyyy-MM-dd"))
                        .ToDictionary(g => g.Key, g => g.Count()),
                    CategoryStats = categoryStats,
                    ChannelStats = channelStats,
                    TotalSentThisMonth = totalSentThisMonth,
                    TotalReadThisMonth = totalReadThisMonth,
                    ReadRateThisMonth = readRateThisMonth,
                    EmailsSentThisMonth = thisMonthNotifications.Count(n => n.EmailSent)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification history");
                TempData["Error"] = "An error occurred while loading history.";
                return View(new NotificationHistoryViewModel());
            }
        }
    }
}