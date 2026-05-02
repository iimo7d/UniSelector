using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.ApplicationReview;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "UniversityRep")]
    [Route("UniversityRep/Applications")]
    public class ApplicationReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public ApplicationReviewController(
            AppDbContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        // GET: /UniversityRep/Applications
        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, ApplicationStatus? status, int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null)
            {
                TempData["Error"] = "You are not assigned to any university.";
                return RedirectToAction("Index", "UniversityRep");
            }

            if (!rep.CanViewApplications)
            {
                TempData["Error"] = "You do not have permission to view applications.";
                return RedirectToAction("Index", "UniversityRep");
            }

            // Query applications for this university
            var query = _context.StudentApplications
                .Include(sa => sa.Student)
                .ThenInclude(s => s.User)
                .Include(sa => sa.UniversityProgram)
                .ThenInclude(up => up.Program)
                .Include(sa => sa.BtecProgram)
                .Where(sa => (sa.UniversityProgram != null && sa.UniversityProgram.UniversityId == rep.UniversityId) ||
                             (sa.BtecProgram != null && sa.BtecProgram.UniversityId == rep.UniversityId));

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(sa =>
                    sa.ApplicationNumber.Contains(search) ||
                    sa.Student.User.FullName.Contains(search) ||
                    sa.Student.User.Email.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(sa => sa.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var applications = await query
                .OrderByDescending(sa => sa.ApplicationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sa => new ApplicationListItemDto
                {
                    Id = sa.Id,
                    ApplicationNumber = sa.ApplicationNumber,
                    StudentName = sa.Student.User.FullName,
                    StudentEmail = sa.Student.User.Email,
                    ProgramName = sa.UniversityProgram != null ? sa.UniversityProgram.Program.NameEnglish : sa.BtecProgram.NameEnglish,
                    IsBtecProgram = sa.BtecProgramId != null,
                    Status = sa.Status,
                    ApplicationDate = sa.ApplicationDate,
                    ApprovalDate = sa.ApprovalDate,
                    StudentGPA = sa.Student.GPA,
                    StudentPath = sa.Student.Path
                })
                .ToListAsync();

            var viewModel = new ApplicationListViewModel
            {
                Applications = applications,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Search = search,
                StatusFilter = status,
                UniversityId = rep.UniversityId,
                UniversityName = rep.University.NameEnglish,
                CanViewApplications = rep.CanViewApplications
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/Applications/{id}/Details
        [HttpGet("{id}/Details")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                TempData["Error"] = "Permission denied.";
                return RedirectToAction(nameof(Index));
            }

            var application = await _context.StudentApplications
                .Include(sa => sa.Student)
                .ThenInclude(s => s.User)
                .Include(sa => sa.Student.FamilyConnectionUniversity)
                .Include(sa => sa.UniversityProgram)
                .ThenInclude(up => up.Program)
                .Include(sa => sa.UniversityProgram.University)
                .Include(sa => sa.BtecProgram)
                .ThenInclude(bp => bp.University)
                .Include(sa => sa.DiscountGrant)
                .Include(sa => sa.Commission)
                .Include(sa => sa.HourDiscountSetByUser)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
            {
                TempData["Error"] = "Application not found.";
                return RedirectToAction(nameof(Index));
            }

            // Verify the application belongs to this university
            var universityId = application.UniversityProgram?.UniversityId ?? application.BtecProgram?.UniversityId;
            if (universityId != rep.UniversityId)
            {
                TempData["Error"] = "Application not found.";
                return RedirectToAction(nameof(Index));
            }

            var isBtec = application.BtecProgramId.HasValue;
            var university = application.UniversityProgram?.University ?? application.BtecProgram.University;

            var viewModel = new ApplicationDetailsViewModel
            {
                // Application Info
                ApplicationId = application.Id,
                ApplicationNumber = application.ApplicationNumber,
                AdmissionNumber = application.AdmissionNumber,
                Status = application.Status,
                ApplicationDate = application.ApplicationDate,
                ApprovalDate = application.ApprovalDate,
                UpdatedAt = application.UpdatedAt,
                Notes = application.Notes,
                RejectionReason = application.RejectionReason,
                PlannedFirstSemesterHours = application.PlannedFirstSemesterHours,
                HourDiscountPercent = application.HourDiscountPercent,
                HourDiscountSetByUserName = application.HourDiscountSetByUser?.FullName,
                HourDiscountSetAt = application.HourDiscountSetAt,

                // Student Info
                StudentId = application.Student.Id,
                StudentName = application.Student.User.FullName,
                StudentEmail = application.Student.User.Email,
                StudentPhone = application.Student.User.PhoneNumber,
                NationalId = application.Student.NationalId,
                SeatNumber = application.Student.SeatNumber,
                Gender = application.Student.Gender,
                DateOfBirth = application.Student.DateOfBirth,
                Nationality = application.Student.Nationality,
                GPA = application.Student.GPA,
                Path = application.Student.Path,
                AcademicTrack = application.Student.AcademicTrack,
                VocationalBranch = application.Student.VocationalBranch,
                BtecLevel2Completed = application.Student.BtecLevel2Completed,
                BtecLevel3Completed = application.Student.BtecLevel3Completed,
                BtecCertificateUrl = application.Student.BtecCertificateUrl,

                // Guardian Info
                GuardianName = application.Student.GuardianName,
                GuardianPhone = application.Student.GuardianPhone,
                GuardianRelation = application.Student.GuardianRelation,

                // Location
                Province = application.Student.Province,
                City = application.Student.City,
                Area = application.Student.Area,

                // Preferences
                RegistrationBudget = application.Student.RegistrationBudget,
                DesiredMajors = application.Student.DesiredMajors,
                PreferredCity = application.Student.PreferredCity,
                PreferredLanguage = application.Student.PreferredLanguage,
                HasFamilyConnection = application.Student.HasFamilyConnection,
                FamilyConnectionUniversityName = application.Student.FamilyConnectionUniversity?.NameEnglish,

                // Program Info
                IsBtecProgram = isBtec,
                ProgramNameArabic = isBtec ? application.BtecProgram.NameArabic : application.UniversityProgram.Program.NameArabic,
                ProgramNameEnglish = isBtec ? application.BtecProgram.NameEnglish : application.UniversityProgram.Program.NameEnglish,
                ProgramDescription = isBtec ? application.BtecProgram.Description : application.UniversityProgram.Program.Description,
                Degree = isBtec ? null : (Degree?)application.UniversityProgram.Program.Degree,
                BtecLevel = isBtec ? (BtecLevel?)application.BtecProgram.Level : null,
                TechnicalField = isBtec ? application.BtecProgram.TechnicalField : null,
                ProgramLanguage = isBtec ? application.BtecProgram.Language : application.UniversityProgram.Program.Language,
                ProgramDuration = isBtec ? application.BtecProgram.DurationInYears : application.UniversityProgram.DurationInYears,
                TotalCreditHours = isBtec ? application.BtecProgram.TotalCreditHours : application.UniversityProgram.Program.TotalCreditHours,
                HourPriceBase = isBtec ? application.BtecProgram.HourPriceBase : application.UniversityProgram.HourPriceBase,
                RegistrationFeeFirstSemester = isBtec ? application.BtecProgram.RegistrationFeeFirstSemester : application.UniversityProgram.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = isBtec ? application.BtecProgram.RegistrationFeeRegularSemester : application.UniversityProgram.RegistrationFeeRegularSemester,
                StudySystem = isBtec ? null : (StudySystem?)application.UniversityProgram.StudySystem,
                IsProgramApprovedByBtec = isBtec && application.BtecProgram.IsApprovedByBtecAuthority,

                // Discount & Commission
                HasDiscountGrant = application.DiscountGrant != null,
                DiscountCode = application.DiscountGrant?.Code,
                DiscountPercentage = application.DiscountGrant?.Percentage,
                DiscountAmountEstimated = application.DiscountGrant?.AmountEstimated,
                DiscountStatus = application.DiscountGrant?.Status,
                DiscountGrantedAt = application.DiscountGrant?.GrantedAt,

                HasCommission = application.Commission != null,
                CommissionPercentage = application.Commission?.Percentage,
                CommissionAmountEstimated = application.Commission?.AmountEstimated,
                CommissionMode = application.Commission?.Mode,
                CommissionSettled = application.Commission?.Settled,

                // University Info
                UniversityId = university.Id,
                UniversityNameArabic = university.NameArabic,
                UniversityNameEnglish = university.NameEnglish,
                UniversityCity = university.City,
                UniversityPhone = university.PhoneNumber,
                UniversityEmail = university.Email,

                // Permissions
                CanViewApplications = rep.CanViewApplications
            };

            return View(viewModel);
        }

        // POST: /UniversityRep/Applications/{id}/Approve
        [HttpPost("{id}/Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, ApproveApplicationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var application = await _context.StudentApplications
                .Include(sa => sa.Student)
                .ThenInclude(s => s.User)
                .Include(sa => sa.UniversityProgram)
                .ThenInclude(up => up.Program)
                .Include(sa => sa.BtecProgram)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Verify the application belongs to this university
            var universityId = application.UniversityProgram?.UniversityId ?? application.BtecProgram?.UniversityId;
            if (universityId != rep.UniversityId)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Check if application can be approved
            if (application.Status != ApplicationStatus.Pending && application.Status != ApplicationStatus.UnderReview)
            {
                return Json(new { success = false, message = "Application cannot be approved in its current status." });
            }

            // Get university for commission mode
            var university = rep.University;
            var isBtec = application.BtecProgramId.HasValue;

            // Update application status
            application.Status = ApplicationStatus.Approved;
            application.ApprovalDate = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(model.ApprovalNotes))
            {
                application.Notes = string.IsNullOrWhiteSpace(application.Notes)
                    ? model.ApprovalNotes
                    : application.Notes + "\n\n[Approval] " + model.ApprovalNotes;
            }

            // Generate Admission Number
            if (string.IsNullOrWhiteSpace(application.AdmissionNumber))
            {
                application.AdmissionNumber = GenerateAdmissionNumber(university.Id);
            }

            // Create DiscountGrant (5% automatic discount)
            var discountGrant = new DiscountGrant
            {
                ApplicationId = application.Id,
                Code = GenerateDiscountCode(university.Id),
                UniversityId = university.Id,
                Percentage = 5m, // 5% automatic discount
                AmountEstimated = CalculateDiscountAmount(application, university.CommissionMode, 5m),
                Status = DiscountStatus.Issued,
                GrantedAt = DateTime.UtcNow
            };

            _context.DiscountGrants.Add(discountGrant);

            // Create Commission (2% based on CommissionMode)
            var commission = new Commission
            {
                ApplicationId = application.Id,
                UniversityId = university.Id,
                Mode = university.CommissionMode,
                Percentage = 2m,
                BaseAmount = CalculateCommissionBaseAmount(application, university.CommissionMode),
                AmountEstimated = CalculateCommissionAmount(application, university.CommissionMode),
                Settled = false,
                CreatedAt = DateTime.UtcNow,
                CalculatedAt = DateTime.UtcNow
            };

            // Set commission details based on mode
            SetCommissionDetails(commission, application, university.CommissionMode);

            _context.Commissions.Add(commission);

            await _context.SaveChangesAsync();

            // ===================================
            // 🔔 SEND NOTIFICATIONS & EMAILS
            // ===================================

            try
            {
                var studentUserId = application.Student.UserId;
                var studentEmail = application.Student.User.Email;
                var studentName = application.Student.User.FullName;
                var programName = isBtec
                    ? application.BtecProgram.NameEnglish
                    : application.UniversityProgram.Program.NameEnglish;
                var applicationDetailsUrl = $"/Applications/{application.Id}/Details";

                // 1️⃣ Notify Student: Application Approved (In-App + Email)
                await _notificationService.SendNotificationAsync(
                    userId: studentUserId,
                    title: "🎉 Application Approved!",
                    message: $"Congratulations! Your application for {programName} at {university.NameEnglish} has been approved. Admission Number: {application.AdmissionNumber}",
                    category: NotificationCategory.ApplicationApproved,
                    channel: NotificationChannel.InApp,
                    actionUrl: applicationDetailsUrl
                );

                // Send Email: Application Approved
                await _emailService.SendApplicationApprovedEmailAsync(
                    studentEmail,
                    studentName,
                    programName,
                    university.NameEnglish,
                    application.AdmissionNumber,
                    discountGrant.Code,
                    discountGrant.AmountEstimated
                );

                // 2️⃣ Notify Student: Discount Code Generated
                await _notificationService.SendNotificationAsync(
                    userId: studentUserId,
                    title: "💰 Discount Code Generated!",
                    message: $"You have received a 5% discount! Use code: {discountGrant.Code}. Estimated savings: {discountGrant.AmountEstimated:N2} JOD.",
                    category: NotificationCategory.PromoCodeGenerated,
                    channel: NotificationChannel.InApp,
                    actionUrl: applicationDetailsUrl
                );

                // 3️⃣ Notify University: Commission Generated
                await _notificationService.SendNotificationToUniversityAsync(
                    universityId: university.Id,
                    title: "💵 Commission Generated",
                    message: $"Commission of {commission.AmountEstimated:N2} JOD generated for {application.Student.User.FullName}'s application to {programName}.",
                    category: NotificationCategory.CommissionGenerated,
                    channel: NotificationChannel.InApp,
                    actionUrl: $"/UniversityRep/Commissions/{commission.Id}"
                );
            }
            catch (Exception ex)
            {
                // Log notification errors but don't fail the approval
                Console.WriteLine($"Notification/Email error: {ex.Message}");
            }

            TempData["Success"] = "Application approved successfully. Discount code and commission generated.";
            return Json(new { success = true, message = "Application approved successfully." });
        }

        // POST: /UniversityRep/Applications/{id}/Reject
        [HttpPost("{id}/Reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, RejectApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Rejection reason is required." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var application = await _context.StudentApplications
                .Include(sa => sa.Student)
                .ThenInclude(s => s.User)
                .Include(sa => sa.UniversityProgram)
                .ThenInclude(up => up.Program)
                .Include(sa => sa.BtecProgram)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Verify the application belongs to this university
            var universityId = application.UniversityProgram?.UniversityId ?? application.BtecProgram?.UniversityId;
            if (universityId != rep.UniversityId)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Check if application can be rejected
            if (application.Status != ApplicationStatus.Pending && application.Status != ApplicationStatus.UnderReview)
            {
                return Json(new { success = false, message = "Application cannot be rejected in its current status." });
            }

            application.Status = ApplicationStatus.Rejected;
            application.RejectionReason = model.RejectionReason;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ===================================
            // 🔔 SEND NOTIFICATION & EMAIL
            // ===================================

            try
            {
                var studentUserId = application.Student.UserId;
                var studentEmail = application.Student.User.Email;
                var studentName = application.Student.User.FullName;
                var isBtec = application.BtecProgramId.HasValue;
                var programName = isBtec
                    ? application.BtecProgram.NameEnglish
                    : application.UniversityProgram.Program.NameEnglish;
                var applicationDetailsUrl = $"/Applications/{application.Id}/Details";

                // Notify Student: Application Rejected (In-App)
                await _notificationService.SendNotificationAsync(
                    userId: studentUserId,
                    title: "❌ Application Status Update",
                    message: $"Your application for {programName} at {rep.University.NameEnglish} has been rejected. Reason: {model.RejectionReason}",
                    category: NotificationCategory.ApplicationRejected,
                    channel: NotificationChannel.InApp,
                    actionUrl: applicationDetailsUrl
                );

                // Send Email: Application Rejected
                await _emailService.SendApplicationRejectedEmailAsync(
                    studentEmail,
                    studentName,
                    programName,
                    rep.University.NameEnglish,
                    model.RejectionReason
                );
            }
            catch (Exception ex)
            {
                // Log notification errors but don't fail the rejection
                Console.WriteLine($"Notification/Email error: {ex.Message}");
            }

            TempData["Success"] = "Application rejected.";
            return Json(new { success = true, message = "Application rejected successfully." });
        }

        // POST: /UniversityRep/Applications/{id}/SetDiscount
        [HttpPost("{id}/SetDiscount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDiscount(int id, SetDiscountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var application = await _context.StudentApplications
                .Include(sa => sa.Student)
                .ThenInclude(s => s.User)
                .Include(sa => sa.UniversityProgram)
                .ThenInclude(up => up.Program)
                .Include(sa => sa.BtecProgram)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Verify the application belongs to this university
            var universityId = application.UniversityProgram?.UniversityId ?? application.BtecProgram?.UniversityId;
            if (universityId != rep.UniversityId)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Check if application is approved or enrolled
            if (application.Status != ApplicationStatus.Approved && application.Status != ApplicationStatus.Enrolled)
            {
                return Json(new { success = false, message = "Can only set discount for approved or enrolled applications." });
            }

            application.HourDiscountPercent = model.HourDiscountPercent;
            application.PlannedFirstSemesterHours = model.PlannedFirstSemesterHours;
            application.HourDiscountSetByUserId = userId;
            application.HourDiscountSetAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ===================================
            // 🔔 SEND NOTIFICATION & EMAIL
            // ===================================

            try
            {
                var studentUserId = application.Student.UserId;
                var studentEmail = application.Student.User.Email;
                var studentName = application.Student.User.FullName;
                var isBtec = application.BtecProgramId.HasValue;
                var programName = isBtec
                    ? application.BtecProgram.NameEnglish
                    : application.UniversityProgram.Program.NameEnglish;
                var applicationDetailsUrl = $"/Applications/{application.Id}/Details";

                // Notify Student: Hour Discount Applied (In-App)
                await _notificationService.SendNotificationAsync(
                    userId: studentUserId,
                    title: "🎁 Hour Discount Applied!",
                    message: $"Great news! A {model.HourDiscountPercent}% hour discount has been applied to your {programName} application at {rep.University.NameEnglish}.",
                    category: NotificationCategory.DiscountApplied,
                    channel: NotificationChannel.InApp,
                    actionUrl: applicationDetailsUrl
                );

                // Send Email: Hour Discount Applied
                await _emailService.SendHourDiscountAppliedEmailAsync(
                    studentEmail,
                    studentName,
                    programName,
                    rep.University.NameEnglish,
                    model.HourDiscountPercent
                );
            }
            catch (Exception ex)
            {
                // Log notification errors but don't fail the operation
                Console.WriteLine($"Notification/Email error: {ex.Message}");
            }

            TempData["Success"] = $"Hour discount set to {model.HourDiscountPercent}%.";
            return Json(new { success = true, message = "Discount set successfully." });
        }

        // POST: /UniversityRep/Applications/{id}/Notes
        [HttpPost("{id}/Notes")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNotes(int id, AddNotesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Notes are required." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var application = await _context.StudentApplications
                .Include(sa => sa.UniversityProgram)
                .Include(sa => sa.BtecProgram)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Verify the application belongs to this university
            var universityId = application.UniversityProgram?.UniversityId ?? application.BtecProgram?.UniversityId;
            if (universityId != rep.UniversityId)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Append notes with timestamp
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "University Rep";
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
            var newNote = $"[{timestamp}] {userName}: {model.Notes}";

            application.Notes = string.IsNullOrWhiteSpace(application.Notes)
                ? newNote
                : application.Notes + "\n\n" + newNote;

            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Notes added successfully.";
            return Json(new { success = true, message = "Notes added successfully.", notes = application.Notes });
        }

        // Helper Methods
        private string GenerateDiscountCode(int universityId)
        {
            var year = DateTime.UtcNow.Year;
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"DISC-UNI{universityId}-{year}-{uniqueId}";
        }

        private string GenerateAdmissionNumber(int universityId)
        {
            var year = DateTime.UtcNow.Year;
            var count = _context.StudentApplications
                .Count(sa => sa.AdmissionNumber != null &&
                             sa.AdmissionNumber.StartsWith($"ADM-UNI{universityId}-{year}"));
            return $"ADM-UNI{universityId}-{year}-{(count + 1):D4}";
        }

        private decimal CalculateDiscountAmount(StudentApplication application, CommissionMode mode, decimal discountPercent)
        {
            var baseAmount = CalculateCommissionBaseAmount(application, mode);
            return baseAmount * (discountPercent / 100m);
        }

        private decimal CalculateCommissionBaseAmount(StudentApplication application, CommissionMode mode)
        {
            var isBtec = application.BtecProgramId.HasValue;

            switch (mode)
            {
                case CommissionMode.FirstSemesterRegistration2Percent:
                    return isBtec ? application.BtecProgram.RegistrationFeeFirstSemester
                                  : application.UniversityProgram.RegistrationFeeFirstSemester;

                case CommissionMode.ProgramTotalHours2Percent:
                    var totalHours = isBtec ? application.BtecProgram.TotalCreditHours
                                            : application.UniversityProgram.Program.TotalCreditHours;
                    var hourPrice = isBtec ? application.BtecProgram.HourPriceBase
                                           : application.UniversityProgram.HourPriceBase;
                    return totalHours * hourPrice;

                case CommissionMode.FirstSemesterRegPlusHours2Percent:
                    var regFee = isBtec ? application.BtecProgram.RegistrationFeeFirstSemester
                                        : application.UniversityProgram.RegistrationFeeFirstSemester;
                    var plannedHours = application.PlannedFirstSemesterHours ?? 15; // Default 15 hours
                    var price = isBtec ? application.BtecProgram.HourPriceBase
                                       : application.UniversityProgram.HourPriceBase;
                    return regFee + (plannedHours * price);

                default:
                    return 0;
            }
        }

        private decimal CalculateCommissionAmount(StudentApplication application, CommissionMode mode)
        {
            var baseAmount = CalculateCommissionBaseAmount(application, mode);
            return baseAmount * 0.02m; // 2% commission
        }

        private void SetCommissionDetails(Commission commission, StudentApplication application, CommissionMode mode)
        {
            var isBtec = application.BtecProgramId.HasValue;

            switch (mode)
            {
                case CommissionMode.FirstSemesterRegistration2Percent:
                    commission.RegistrationFeeUsed = isBtec ? application.BtecProgram.RegistrationFeeFirstSemester
                                                            : application.UniversityProgram.RegistrationFeeFirstSemester;
                    break;

                case CommissionMode.ProgramTotalHours2Percent:
                    commission.HoursCountUsed = isBtec ? application.BtecProgram.TotalCreditHours
                                                       : application.UniversityProgram.Program.TotalCreditHours;
                    commission.HourPriceUsed = isBtec ? application.BtecProgram.HourPriceBase
                                                      : application.UniversityProgram.HourPriceBase;
                    break;

                case CommissionMode.FirstSemesterRegPlusHours2Percent:
                    commission.RegistrationFeeUsed = isBtec ? application.BtecProgram.RegistrationFeeFirstSemester
                                                            : application.UniversityProgram.RegistrationFeeFirstSemester;
                    commission.HoursCountUsed = application.PlannedFirstSemesterHours ?? 15;
                    commission.HourPriceUsed = isBtec ? application.BtecProgram.HourPriceBase
                                                      : application.UniversityProgram.HourPriceBase;
                    break;
            }
        }
    }
}
