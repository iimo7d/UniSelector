using Microsoft.AspNetCore.Identity;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Service.Implementation
{
    public class BtecStandardsNotifier : IBtecStandardsNotifier
    {
        private readonly ILogger<BtecStandardsNotifier> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public BtecStandardsNotifier(
            ILogger<BtecStandardsNotifier> logger,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _logger = logger;
            _userManager = userManager;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task SendStandardsUpdateNotificationsAsync(UpdateBTECStandardsViewModel model, ApplicationUser updatedBy)
        {
            var universityReps = await _userManager.GetUsersInRoleAsync("UniversityRep");
            if (!universityReps.Any())
            {
                _logger.LogWarning("No university representatives found to notify");
                return;
            }

            var notificationTitle = "⚠️ BTEC Standards Updated";
            var notificationMessage =
                $"The BTEC Authority has updated the BTEC standards. Update: {model.UpdateDescription}. " +
                $"Effective Date: {model.EffectiveDate:MMMM dd, yyyy}.";

            // In-app
            await _notificationService.SendNotificationToRoleAsync(
                "UniversityRep",
                notificationTitle,
                notificationMessage,
                NotificationCategory.SystemAlert,
                NotificationChannel.InApp,
                actionUrl: "/BTECAuthority/Standards"
            );

            // Emails
            var tasks = universityReps
                .Where(r => !string.IsNullOrEmpty(r.Email))
                .Select(r => _emailService.SendStandardsUpdateEmailAsync(
                    r.Email!,
                    r.FullName,
                    model.UpdateDescription,
                    model.EffectiveDate,
                    string.IsNullOrWhiteSpace(model.NotificationMessage)
                        ? "Please review the new standards and ensure your BTEC programs comply."
                        : model.NotificationMessage
                ));

            await Task.WhenAll(tasks);
        }
    }

}
