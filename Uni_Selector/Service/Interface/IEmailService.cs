using Uni_Selector.Models;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Service.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailAsync(string to, string subject, string body, bool isHtml);
        Task SendBulkEmailAsync(List<string> recipients, string subject, string body);

        Task SendEmailConfirmationAsync(string email, string fullName, string confirmationUrl);
        Task SendPasswordResetAsync(string email, string fullName, string resetUrl);
        Task SendWelcomeEmailAsync(string email, string fullName);
        Task SendAccountLockedNotificationAsync(string email, string fullName, DateTime unlockTime);
        Task SendPasswordChangedNotificationAsync(string email, string fullName);

        Task SendApplicationApprovedEmailAsync(string email, string studentName, string programName, string universityName, string admissionNumber, string discountCode, decimal discountAmount);
        Task SendApplicationRejectedEmailAsync(string email, string studentName, string programName, string universityName, string rejectionReason);
        Task SendHourDiscountAppliedEmailAsync(string email, string studentName, string programName, string universityName, decimal discountPercentage);


        Task SendProgramRejectedEmailAsync(string email, string recipientName, string programName, string universityName, string level, string rejectionReason);
        Task SendProgramApprovedEmailAsync(string email, string recipientName, string programName, string universityName, string level, string approvalNotes);

        Task SendStandardsUpdateEmailAsync(string email, string recipientName, string updateDescription, DateTime effectiveDate, string customMessage);
    }
}
