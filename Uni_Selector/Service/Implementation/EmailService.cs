using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            await SendEmailAsync(to, subject, body, isHtml: true);
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml)
        {
            var sendGridKey = _configuration["EmailSettings:SendGridApiKey"];
            var isSendGridConfigured = !string.IsNullOrWhiteSpace(sendGridKey)
                                       && sendGridKey != "Send Grid API Key";

            if (isSendGridConfigured)
            {
                try
                {
                    await SendViaSendGridAsync(to, subject, body, isHtml);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "[SendGrid] Failed to send email to {To}. Falling back to Gmail SMTP.", to);
                }
            }
            else
            {
                _logger.LogInformation(
                    "[EmailService] SendGrid API key not configured. Sending via Gmail SMTP directly.");
            }

            await SendViaGmailAsync(to, subject, body, isHtml);
        }

        // ── Provider: SendGrid ──────────────────────────────────────────────────────
        private async Task SendViaSendGridAsync(string to, string subject, string body, bool isHtml)
        {
            var apiKey      = _configuration["EmailSettings:SendGridApiKey"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName  = _configuration["EmailSettings:SenderName"];

            var client     = new SendGridClient(apiKey);
            var from       = new EmailAddress(senderEmail, senderName);
            var toAddress  = new EmailAddress(to);

            var msg = new SendGridMessage { From = from, Subject = subject };
            msg.AddTo(toAddress);

            if (isHtml)
                msg.HtmlContent = body;
            else
                msg.PlainTextContent = body;

            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "[SendGrid] Email sent successfully to {To}. Status: {Status}", to, response.StatusCode);
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError(
                    "[SendGrid] Failed to send to {To}. Status: {Status}, Body: {ErrorBody}",
                    to, response.StatusCode, errorBody);
                throw new Exception($"SendGrid API Error: {response.StatusCode}");
            }
        }

        // ── Provider: Gmail SMTP (MailKit) ──────────────────────────────────────────
        private async Task SendViaGmailAsync(string to, string subject, string body, bool isHtml)
        {
            var smtpServer  = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort    = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var p) ? p : 587;
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName  = _configuration["EmailSettings:SenderName"] ?? "Smart University Platform";
            var username    = _configuration["EmailSettings:Username"];
            var password    = _configuration["EmailSettings:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(
                    "Gmail SMTP credentials (EmailSettings:Username / EmailSettings:Password) are not configured.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail ?? username));
            message.To.Add(new MailboxAddress(string.Empty, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
                bodyBuilder.HtmlBody = body;
            else
                bodyBuilder.TextBody = body;

            message.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(username, password);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);

            _logger.LogInformation(
                "[Gmail SMTP] Email sent successfully to {To} via {SmtpServer}:{SmtpPort}",
                to, smtpServer, smtpPort);
        }

        public async Task SendBulkEmailAsync(List<string> recipients, string subject, string body)
        {
            // Note: SendGrid supports true bulk sending (Personalizations), 
            // but for simplicity and to match your previous logic, we can loop.
            // Be aware of SendGrid rate limits if this list is very large.
            foreach (var recipient in recipients)
            {
                try
                {
                    await SendEmailAsync(recipient, subject, body);
                    // Minimal delay not strictly necessary for SendGrid API, but keeps pace even
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send bulk email to {recipient}");
                }
            }
        }

        #region Authentication Email Templates

        public async Task SendEmailConfirmationAsync(string email, string fullName, string confirmationUrl)
        {
            var subject = "Confirm Your Email Address";
            var body = GetEmailConfirmationTemplate(fullName, confirmationUrl);

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        public async Task SendPasswordResetAsync(string email, string fullName, string resetUrl)
        {
            var subject = "Reset Your Password";
            var body = GetPasswordResetTemplate(fullName, resetUrl);

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        public async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var subject = "Welcome to Our Platform!";
            var body = GetWelcomeEmailTemplate(fullName);

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        public async Task SendAccountLockedNotificationAsync(string email, string fullName, DateTime unlockTime)
        {
            var subject = "Account Security Alert - Account Locked";
            var body = GetAccountLockedTemplate(fullName, unlockTime);

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
        {
            var subject = "Password Changed Successfully";
            var body = GetPasswordChangedTemplate(fullName);

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        #endregion

        #region Email Template Builders
        // ... (The template methods below remain exactly the same as your original code) ...

        private string GetEmailConfirmationTemplate(string fullName, string confirmationUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 40px 30px; }}
        .content h2 {{ color: #667eea; margin-top: 0; }}
        .button {{ display: inline-block; padding: 14px 40px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .button:hover {{ background: #5568d3; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome Aboard!</h1>
        </div>
        <div class='content'>
            <h2>Hi {fullName},</h2>
            <p>Thank you for creating an account with us! We're excited to have you on board.</p>
            <p>To get started, please confirm your email address by clicking the button below:</p>
            
            <div style='text-align: center;'>
                <a href='{confirmationUrl}' class='button'>Confirm Email Address</a>
            </div>

            <div class='warning'>
                <strong>⏰ Quick Reminder:</strong> This link will expire in 24 hours for security purposes.
            </div>

            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #667eea; font-size: 14px;'>{confirmationUrl}</p>

            <p>If you didn't create this account, you can safely ignore this email.</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Your Platform Name. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetTemplate(string fullName, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 40px 30px; }}
        .content h2 {{ color: #f5576c; margin-top: 0; }}
        .button {{ display: inline-block; padding: 14px 40px; background: #f5576c; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .button:hover {{ background: #e04558; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
        .alert {{ background: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0; color: #721c24; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hi {fullName},</h2>
            <p>We received a request to reset your password. No worries, we've got you covered!</p>
            <p>Click the button below to create a new password:</p>
            
            <div style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Reset Password</a>
            </div>

            <div class='alert'>
                <strong>⚠️ Security Notice:</strong> This link will expire in 1 hour. If you didn't request this reset, please ignore this email and your password will remain unchanged.
            </div>

            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #f5576c; font-size: 14px;'>{resetUrl}</p>

            <p><strong>For your security:</strong></p>
            <ul>
                <li>Never share this link with anyone</li>
                <li>We will never ask for your password via email</li>
                <li>If you suspect unauthorized access, contact support immediately</li>
            </ul>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Your Platform Name. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string fullName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 40px 30px; }}
        .feature-box {{ background: #f8f9fa; padding: 20px; margin: 15px 0; border-radius: 8px; border-left: 4px solid #667eea; }}
        .button {{ display: inline-block; padding: 14px 40px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎊 Welcome to the Platform!</h1>
        </div>
        <div class='content'>
            <h2>Hi {fullName},</h2>
            <p>Your email has been confirmed successfully! You're all set to explore everything we have to offer.</p>
            
            <div class='feature-box'>
                <h3 style='margin-top: 0; color: #667eea;'>🚀 Quick Start Guide</h3>
                <ul style='padding-left: 20px;'>
                    <li>Complete your profile</li>
                    <li>Explore available features</li>
                    <li>Connect with the community</li>
                    <li>Check out our tutorials</li>
                </ul>
            </div>

            <div style='text-align: center;'>
                <a href='#' class='button'>Get Started Now</a>
            </div>

            <p>Need help? Our support team is here for you 24/7. Just reply to this email or visit our help center.</p>
            
            <p>Best regards,<br><strong>The Team</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Your Platform Name. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetAccountLockedTemplate(string fullName, DateTime unlockTime)
        {
            var unlockTimeStr = unlockTime.ToString("MMM dd, yyyy hh:mm tt");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #f5576c 0%, #d63447 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 40px 30px; }}
        .alert {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔒 Account Temporarily Locked</h1>
        </div>
        <div class='content'>
            <h2>Hi {fullName},</h2>
            <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
            
            <div class='alert'>
                <strong>🕐 Unlock Time:</strong> {unlockTimeStr}
            </div>

            <p><strong>What happened?</strong></p>
            <p>We detected 5 consecutive failed login attempts. As a security measure, we've temporarily locked your account for 15 minutes.</p>

            <p><strong>What should you do?</strong></p>
            <ul>
                <li>Wait until {unlockTimeStr} to try again</li>
                <li>Make sure you're using the correct password</li>
                <li>If you forgot your password, use the 'Forgot Password' option</li>
                <li>If you didn't attempt to log in, contact support immediately</li>
            </ul>

            <p>If you believe this was a mistake or need immediate assistance, please contact our support team.</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Your Platform Name. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordChangedTemplate(string fullName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 40px 30px; }}
        .success {{ background: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; color: #155724; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Password Changed Successfully</h1>
        </div>
        <div class='content'>
            <h2>Hi {fullName},</h2>
            
            <div class='success'>
                <strong>✓ Success!</strong> Your password has been changed successfully on {DateTime.Now:MMM dd, yyyy} at {DateTime.Now:hh:mm tt}.
            </div>

            <p><strong>⚠️ Didn't make this change?</strong></p>
            <p>If you didn't change your password, please contact our support team immediately. Your account security is our top priority.</p>

            <p><strong>Security Reminders:</strong></p>
            <ul>
                <li>Never share your password with anyone</li>
                <li>Use a unique password for this account</li>
                <li>Change your password regularly</li>
                <li>Enable two-factor authentication if available</li>
            </ul>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Your Platform Name. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion

        #region Discount and Approval 
        public async Task SendApplicationApprovedEmailAsync(
            string email,
            string studentName,
            string programName,
            string universityName,
            string admissionNumber,
            string discountCode,
            decimal discountAmount)
        {
            var subject = "🎉 Application Approved - Welcome to " + universityName;
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; }}
        .content {{ padding: 40px 30px; }}
        .highlight-box {{ background: #f0f7ff; border-left: 4px solid #667eea; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .discount-box {{ background: #d4edda; border-left: 4px solid #28a745; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .code {{ font-size: 24px; font-weight: bold; color: #28a745; letter-spacing: 2px; font-family: monospace; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Congratulations!</h1>
            <p style='font-size: 18px; margin: 10px 0 0 0;'>Your Application Has Been Approved</p>
        </div>
        <div class='content'>
            <h2>Dear {studentName},</h2>
            <p>We are delighted to inform you that your application has been <strong>approved</strong>!</p>
            
            <div class='highlight-box'>
                <h3 style='margin-top: 0; color: #667eea;'>📚 Application Details</h3>
                <p><strong>Program:</strong> {programName}</p>
                <p><strong>University:</strong> {universityName}</p>
                <p><strong>Admission Number:</strong> <span style='color: #667eea; font-weight: bold;'>{admissionNumber}</span></p>
                <p><strong>Status:</strong> <span style='color: #28a745; font-weight: bold;'>✓ APPROVED</span></p>
            </div>

            <div class='discount-box'>
                <h3 style='margin-top: 0; color: #28a745;'>💰 Special Discount for You!</h3>
                <p>As part of your approval, you have received an automatic <strong>5% discount</strong> on your tuition fees!</p>
                <p><strong>Discount Code:</strong></p>
                <p class='code'>{discountCode}</p>
                <p><strong>Estimated Savings:</strong> {discountAmount:N2} JOD</p>
                <p style='font-size: 13px; color: #666;'>Keep this code safe - you'll need it during enrollment!</p>
            </div>

            <h3>📋 Next Steps:</h3>
            <ol style='padding-left: 20px;'>
                <li>Check your student portal for detailed enrollment information</li>
                <li>Review the program requirements and schedule</li>
                <li>Complete the enrollment process before the deadline</li>
                <li>Use your discount code during payment</li>
                <li>Prepare required documents for registration</li>
            </ol>

            <p style='margin-top: 30px;'>If you have any questions or need assistance, please don't hesitate to contact us.</p>
            
            <p>Best regards,<br><strong>{universityName} Admissions Team</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Smart University Platform. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        public async Task SendApplicationRejectedEmailAsync(
            string email,
            string studentName,
            string programName,
            string universityName,
            string rejectionReason)
        {
            var subject = "Application Status Update - " + universityName;
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #f5576c 0%, #d63447 100%); color: white; padding: 40px 20px; text-align: center; }}
        .content {{ padding: 40px 30px; }}
        .info-box {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Application Status Update</h1>
        </div>
        <div class='content'>
            <h2>Dear {studentName},</h2>
            <p>Thank you for your interest in {programName} at {universityName}.</p>
            
            <p>After careful review of your application, we regret to inform you that we are unable to approve your application at this time.</p>

            <div class='info-box'>
                <h3 style='margin-top: 0;'>Reason:</h3>
                <p><strong>{rejectionReason}</strong></p>
            </div>

            <h3>What you can do:</h3>
            <ul style='padding-left: 20px;'>
                <li>Review the entry requirements for this program</li>
                <li>Consider applying to other programs that match your qualifications</li>
                <li>Improve your qualifications and reapply in the future</li>
                <li>Contact us for more information about alternative programs</li>
            </ul>

            <p>We encourage you to explore other educational opportunities on our platform that may be a better fit for your academic profile.</p>
            
            <p>Best regards,<br><strong>{universityName} Admissions Team</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Smart University Platform. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        public async Task SendHourDiscountAppliedEmailAsync(
            string email,
            string studentName,
            string programName,
            string universityName,
            decimal discountPercentage)
        {
            var subject = "🎁 Additional Discount Applied - " + universityName;
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 40px 20px; text-align: center; }}
        .content {{ padding: 40px 30px; }}
        .discount-box {{ background: #d4edda; border-left: 4px solid #28a745; padding: 20px; margin: 20px 0; border-radius: 5px; text-align: center; }}
        .percentage {{ font-size: 48px; font-weight: bold; color: #28a745; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎁 Great News!</h1>
            <p style='font-size: 18px; margin: 10px 0 0 0;'>Additional Discount Applied</p>
        </div>
        <div class='content'>
            <h2>Dear {studentName},</h2>
            <p>We have excellent news for you!</p>

            <div class='discount-box'>
                <h3 style='margin-top: 0; color: #28a745;'>Hour Discount Applied</h3>
                <p class='percentage'>{discountPercentage}%</p>
                <p>This special hour discount has been applied to your application for <strong>{programName}</strong> at {universityName}.</p>
            </div>

            <h3>What this means for you:</h3>
            <ul style='padding-left: 20px;'>
                <li>Reduced cost per credit hour</li>
                <li>Significant savings on your tuition fees</li>
                <li>More affordable education</li>
                <li>Applied automatically during enrollment</li>
            </ul>

            <p>This discount will be applied to your credit hour charges during the enrollment process. No additional action is required from you.</p>

            <p>Congratulations on this additional benefit!</p>
            
            <p>Best regards,<br><strong>{universityName}</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Smart University Platform. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, body, isHtml: true);
        }
        #endregion

        #region Btec Program Approval
        public async Task SendProgramApprovedEmailAsync(
            string email,
            string recipientName,
            string programName,
            string universityName,
            string level,
            string approvalNotes)
        {
            var subject = "✅ BTEC Program Approved - " + programName;
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; }}
        .content {{ padding: 40px 30px; }}
        .success-box {{ background: #d4edda; border-left: 4px solid #28a745; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .info-box {{ background: #f0f7ff; border-left: 4px solid #667eea; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Program Approved!</h1>
            <p style='font-size: 18px; margin: 10px 0 0 0;'>BTEC Authority Approval</p>
        </div>
        <div class='content'>
            <h2>Dear {recipientName},</h2>
            <p>Congratulations! Your BTEC program has been officially <strong>approved</strong> by the BTEC Authority.</p>
            
            <div class='success-box'>
                <h3 style='margin-top: 0; color: #28a745;'>✓ Approval Confirmed</h3>
                <p><strong>Program:</strong> {programName}</p>
                <p><strong>Level:</strong> {level}</p>
                <p><strong>University:</strong> {universityName}</p>
                <p><strong>Status:</strong> <span style='color: #28a745; font-weight: bold;'>APPROVED</span></p>
            </div>

            <div class='info-box'>
                <h3 style='margin-top: 0; color: #667eea;'>📝 Approval Notes</h3>
                <p>{approvalNotes}</p>
            </div>

            <h3>📋 Next Steps:</h3>
            <ul style='padding-left: 20px;'>
                <li>Your program is now visible to students on the platform</li>
                <li>Students can apply to this BTEC program immediately</li>
                <li>Ensure all program details are up to date</li>
                <li>Monitor applications through your university dashboard</li>
                <li>Maintain BTEC standards and quality requirements</li>
            </ul>

            <p style='margin-top: 30px;'>Thank you for your commitment to quality BTEC education.</p>
            
            <p>Best regards,<br><strong>BTEC Authority - Ministry of Education</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Smart University Platform. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, body, isHtml: true);
        }

        public async Task SendProgramRejectedEmailAsync(
            string email,
            string recipientName,
            string programName,
            string universityName,
            string level,
            string rejectionReason)
        {
            var subject = "BTEC Program Status Update - " + programName;
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #f5576c 0%, #d63447 100%); color: white; padding: 40px 20px; text-align: center; }}
        .content {{ padding: 40px 30px; }}
        .warning-box {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>BTEC Program Status Update</h1>
        </div>
        <div class='content'>
            <h2>Dear {recipientName},</h2>
            <p>Thank you for submitting your BTEC program for approval.</p>
            
            <p>After careful review by the BTEC Authority, we regret to inform you that your program <strong>does not meet the approval requirements</strong> at this time.</p>

            <div class='warning-box'>
                <h3 style='margin-top: 0;'>Program Details:</h3>
                <p><strong>Program:</strong> {programName}</p>
                <p><strong>Level:</strong> {level}</p>
                <p><strong>University:</strong> {universityName}</p>
                <p style='margin-top: 15px;'><strong>Reason for Non-Approval:</strong></p>
                <p>{rejectionReason}</p>
            </div>

            <h3>📋 What You Can Do:</h3>
            <ul style='padding-left: 20px;'>
                <li>Review the BTEC Authority requirements carefully</li>
                <li>Address the issues mentioned in the reason above</li>
                <li>Update your program curriculum and structure</li>
                <li>Ensure compliance with BTEC quality standards</li>
                <li>Resubmit your program for approval after making necessary changes</li>
                <li>Contact BTEC Authority for clarification if needed</li>
            </ul>

            <p>We encourage you to address these concerns and resubmit your program for review. Our goal is to maintain the highest standards of technical and vocational education.</p>
            
            <p>Best regards,<br><strong>BTEC Authority - Ministry of Education</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Smart University Platform. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, body, isHtml: true);
        }
        #endregion

        #region Btec Standards Update
        public async Task SendStandardsUpdateEmailAsync(string email, string recipientName, string updateDescription, DateTime effectiveDate, string customMessage)
        {
            try
            {
                var subject = "⚠️ Important: BTEC Standards Update - Action Required";

                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 40px 30px; }}
        .content h2 {{ color: #ff9800; margin-top: 0; }}
        .alert-box {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .info-box {{ background: #f0f7ff; border-left: 4px solid #667eea; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .button {{ display: inline-block; padding: 14px 40px; background: #ff9800; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .button:hover {{ background: #f57c00; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
        ul {{ padding-left: 20px; }}
        li {{ margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ BTEC Standards Update</h1>
            <p style='font-size: 18px; margin: 10px 0 0 0;'>Important Changes to BTEC Requirements</p>
        </div>
        <div class='content'>
            <h2>Dear {recipientName},</h2>
            
            <p>The BTEC Authority has announced an important update to the BTEC standards and requirements that will affect your university's BTEC programs.</p>

            <div class='alert-box'>
                <h3 style='margin-top: 0; color: #ff9800;'>⏰ Key Information</h3>
                <p><strong>Effective Date:</strong> {effectiveDate:MMMM dd, yyyy}</p>
                <p><strong>Update Description:</strong></p>
                <p>{updateDescription}</p>
            </div>

            <div class='info-box'>
                <h3 style='margin-top: 0; color: #667eea;'>📝 Additional Notes</h3>
                <p>{customMessage}</p>
            </div>

            <h3>📋 Required Actions:</h3>
            <ul style='padding-left: 20px;'>
                <li>Review the updated BTEC standards carefully</li>
                <li>Assess your current BTEC programs for compliance</li>
                <li>Make necessary adjustments to meet new requirements</li>
                <li>Update program documentation and materials</li>
                <li>Submit updated program information if required</li>
                <li>Ensure all faculty are aware of the changes</li>
            </ul>

            <h3>⚠️ Important Reminders:</h3>
            <ul style='padding-left: 20px;'>
                <li>Programs must comply with new standards by the effective date</li>
                <li>Non-compliant programs may lose BTEC accreditation</li>
                <li>Contact BTEC Authority if you need clarification</li>
                <li>Allow sufficient time for program modifications</li>
            </ul>

            <div style='text-align: center;'>
                <a href='https://your-platform.com/BTECAuthority/Standards' class='button'>View Updated Standards</a>
            </div>

            <p style='margin-top: 30px;'>If you have any questions about these changes or need assistance with compliance, please don't hesitate to contact the BTEC Authority.</p>
            
            <p>Best regards,<br><strong>BTEC Authority - Ministry of Education</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Smart University Platform. All rights reserved.</p>
            <p><strong>This is an official notification.</strong> Please ensure all relevant staff are informed.</p>
        </div>
    </div>
</body>
</html>";

                await SendEmailAsync(email, subject, body, isHtml: true);
                _logger.LogInformation($"Standards update email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send standards update email to {email}");
                throw; // Re-throw to be caught by WhenAll error handling
            }
        }

        #endregion
    }
}