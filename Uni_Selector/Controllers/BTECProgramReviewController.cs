using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "BtecAuthority")]
    [Route("BTECAuthority/Programs")]
    public class BTECProgramReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public BTECProgramReviewController(
            AppDbContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        // GET: /BTECAuthority/Programs
        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? search,
            BtecLevel? level,
            string? technicalField,
            bool? approvalStatus,
            int? universityId,
            int page = 1,
            int pageSize = 10)
        {
            // Build query
            var query = _context.BtecPrograms
                .Include(bp => bp.University)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(bp =>
                    bp.NameEnglish.Contains(search) ||
                    bp.NameArabic.Contains(search) ||
                    bp.University.NameEnglish.Contains(search) ||
                    bp.TechnicalField.Contains(search));
            }

            if (level.HasValue)
            {
                query = query.Where(bp => bp.Level == level.Value);
            }

            if (!string.IsNullOrWhiteSpace(technicalField))
            {
                query = query.Where(bp => bp.TechnicalField == technicalField);
            }

            if (approvalStatus.HasValue)
            {
                query = query.Where(bp => bp.IsApprovedByBtecAuthority == approvalStatus.Value);
            }

            if (universityId.HasValue)
            {
                query = query.Where(bp => bp.UniversityId == universityId.Value);
            }

            var totalCount = await query.CountAsync();

            var programs = await query
                .OrderByDescending(bp => bp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bp => new BtecProgramListItemDto
                {
                    Id = bp.Id,
                    NameArabic = bp.NameArabic,
                    NameEnglish = bp.NameEnglish,
                    Level = bp.Level,
                    TechnicalField = bp.TechnicalField,
                    Language = bp.Language,
                    UniversityId = bp.UniversityId,
                    UniversityNameEnglish = bp.University.NameEnglish,
                    UniversityNameArabic = bp.University.NameArabic,
                    UniversityCity = bp.University.City,
                    DurationInYears = bp.DurationInYears,
                    TotalCreditHours = bp.TotalCreditHours,
                    HourPriceBase = bp.HourPriceBase,
                    RegistrationFeeFirstSemester = bp.RegistrationFeeFirstSemester,
                    Capacity = bp.Capacity,
                    IsActive = bp.IsActive,
                    IsApprovedByBtecAuthority = bp.IsApprovedByBtecAuthority,
                    ApprovalDate = bp.ApprovalDate,
                    ApprovalNotes = bp.ApprovalNotes,
                    CreatedAt = bp.CreatedAt,
                    UpdatedAt = bp.UpdatedAt
                })
                .ToListAsync();

            // Get filter options
            var universities = await _context.Universities
                .Where(u => u.IsActive && u.BtecPrograms.Any())
                .Select(u => new UniversityOptionDto
                {
                    Id = u.Id,
                    NameEnglish = u.NameEnglish,
                    NameArabic = u.NameArabic,
                    ProgramCount = u.BtecPrograms.Count
                })
                .OrderBy(u => u.NameEnglish)
                .ToListAsync();

            var technicalFields = await _context.BtecPrograms
                .Select(bp => bp.TechnicalField)
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();

            var viewModel = new BtecProgramListViewModel
            {
                Programs = programs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Search = search,
                LevelFilter = level,
                TechnicalFieldFilter = technicalField,
                ApprovalStatusFilter = approvalStatus,
                UniversityIdFilter = universityId,
                Universities = universities,
                TechnicalFields = technicalFields
            };

            return View(viewModel);
        }

        // GET: /BTECAuthority/Programs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var program = await _context.BtecPrograms
                .Include(bp => bp.University)
                .Include(bp => bp.EntryRequirements)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (program == null)
            {
                TempData["Error"] = "BTEC Program not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get statistics
            var totalApplications = await _context.StudentApplications
                .Where(sa => sa.BtecProgramId == id)
                .CountAsync();

            var approvedApplications = await _context.StudentApplications
                .Where(sa => sa.BtecProgramId == id && sa.Status == ApplicationStatus.Approved)
                .CountAsync();

            var enrolledStudents = await _context.StudentApplications
                .Where(sa => sa.BtecProgramId == id && sa.Status == ApplicationStatus.Enrolled)
                .CountAsync();

            // Calculate estimated total cost
            var estimatedTotalCost = (program.TotalCreditHours * program.HourPriceBase) +
                                    program.RegistrationFeeFirstSemester +
                                    (program.RegistrationFeeRegularSemester * (program.DurationInYears * 2 - 1));

            var viewModel = new BtecProgramDetailsViewModel
            {
                // Program Information
                Id = program.Id,
                NameArabic = program.NameArabic,
                NameEnglish = program.NameEnglish,
                Description = program.Description,
                Level = program.Level,
                TechnicalField = program.TechnicalField,
                Language = program.Language,
                DurationInYears = program.DurationInYears,
                SemesterStartDate = program.SemesterStartDate,
                TotalCreditHours = program.TotalCreditHours,

                // Fees
                HourPriceBase = program.HourPriceBase,
                RegistrationFeeFirstSemester = program.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = program.RegistrationFeeRegularSemester,
                EstimatedTotalCost = estimatedTotalCost,

                // Capacity
                Capacity = program.Capacity,

                // University Information
                UniversityId = program.UniversityId,
                UniversityNameArabic = program.University.NameArabic,
                UniversityNameEnglish = program.University.NameEnglish,
                UniversityCity = program.University.City,
                UniversityProvince = program.University.Province,
                UniversityPhone = program.University.PhoneNumber,
                UniversityEmail = program.University.Email,
                UniversityWebsite = program.University.OfficialWebsite,

                // Approval Status
                IsActive = program.IsActive,
                IsApprovedByBtecAuthority = program.IsApprovedByBtecAuthority,
                ApprovalDate = program.ApprovalDate,
                ApprovalNotes = program.ApprovalNotes,
                RejectionReason = program.RejectionReason,

                // Entry Requirements
                EntryRequirements = program.EntryRequirements.Select(er => new BtecEntryRequirementDto
                {
                    Id = er.Id,
                    MinGPA = er.MinGPA,
                    RequiresBtecL2 = er.RequiresBtecL2,
                    RequiresBtecL3 = er.RequiresBtecL3,
                    Notes = er.Notes,
                    EffectiveFrom = er.EffectiveFrom,
                    EffectiveTo = er.EffectiveTo
                }).ToList(),

                // Statistics
                TotalApplications = totalApplications,
                ApprovedApplications = approvedApplications,
                EnrolledStudents = enrolledStudents,

                // Timestamps
                CreatedAt = program.CreatedAt,
                UpdatedAt = program.UpdatedAt
            };

            return View(viewModel);
        }

        // POST: /BTECAuthority/Programs/{id}/Approve
        [HttpPost("{id}/Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, ApproveBtecProgramViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Approval notes are required." });
            }

            var program = await _context.BtecPrograms
                .Include(bp => bp.University)
                .ThenInclude(u => u.Representatives)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (program == null)
            {
                return Json(new { success = false, message = "Program not found." });
            }

            if (program.IsApprovedByBtecAuthority)
            {
                return Json(new { success = false, message = "Program is already approved." });
            }

            // Update program status
            program.IsApprovedByBtecAuthority = true;
            program.IsActive = true;
            program.ApprovalDate = DateTime.UtcNow;
            program.ApprovalNotes = model.ApprovalNotes;
            program.RejectionReason = null;
            program.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ===================================
            // 🔔📧 SEND NOTIFICATIONS & EMAILS
            // ===================================

            try
            {
                var universityName = program.University.NameEnglish;
                var programName = program.NameEnglish;
                var level = $"Level {(int)program.Level}";

                // Notify all university representatives
                var activeReps = program.University.Representatives
                    .Where(r => r.IsActive)
                    .ToList();

                foreach (var rep in activeReps)
                {
                    // 🔔 In-App Notification
                    await _notificationService.SendNotificationAsync(
                        userId: rep.UserId,
                        title: "✅ BTEC Program Approved",
                        message: $"Your BTEC program '{programName}' ({level}) has been approved by BTEC Authority!",
                        category: NotificationCategory.ApplicationApproved,
                        channel: NotificationChannel.InApp,
                        actionUrl: $"/UniversityRep/BTECPrograms/{program.Id}/Details"
                    );

                    // 📧 Email Notification
                    await _emailService.SendProgramApprovedEmailAsync(
                        email: rep.User.Email,
                        recipientName: rep.User.FullName,
                        programName: programName,
                        universityName: universityName,
                        level: level,
                        approvalNotes: model.ApprovalNotes
                    );
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the approval
                Console.WriteLine($"Notification/Email error: {ex.Message}");
            }

            TempData["Success"] = "BTEC Program approved successfully. Notifications sent to university representatives.";
            return Json(new { success = true, message = "Program approved successfully." });
        }

        // POST: /BTECAuthority/Programs/{id}/Reject
        [HttpPost("{id}/Reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, RejectBtecProgramViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Rejection reason is required." });
            }

            var program = await _context.BtecPrograms
                .Include(bp => bp.University)
                .ThenInclude(u => u.Representatives)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (program == null)
            {
                return Json(new { success = false, message = "Program not found." });
            }

            if (program.IsApprovedByBtecAuthority)
            {
                return Json(new { success = false, message = "Cannot reject an already approved program." });
            }

            // Update program status
            program.IsApprovedByBtecAuthority = false;
            program.IsActive = false;
            program.RejectionReason = model.RejectionReason;
            program.ApprovalNotes = null;
            program.ApprovalDate = DateTime.UtcNow;
            program.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ===================================
            // 🔔📧 SEND NOTIFICATIONS & EMAILS
            // ===================================

            try
            {
                var universityName = program.University.NameEnglish;
                var programName = program.NameEnglish;
                var level = $"Level {(int)program.Level}";

                // Notify all university representatives
                var activeReps = program.University.Representatives
                    .Where(r => r.IsActive)
                    .ToList();

                foreach (var rep in activeReps)
                {
                    // 🔔 In-App Notification
                    await _notificationService.SendNotificationAsync(
                        userId: rep.UserId,
                        title: "❌ BTEC Program Rejected",
                        message: $"Your BTEC program '{programName}' ({level}) has been rejected. Reason: {model.RejectionReason}",
                        category: NotificationCategory.ApplicationRejected,
                        channel: NotificationChannel.InApp,
                        actionUrl: $"/UniversityRep/BTECPrograms/{program.Id}/Details"
                    );

                    // 📧 Email Notification
                    await _emailService.SendProgramRejectedEmailAsync(
                        email: rep.User.Email,
                        recipientName: rep.User.FullName,
                        programName: programName,
                        universityName: universityName,
                        level: level,
                        rejectionReason: model.RejectionReason
                    );
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the rejection
                Console.WriteLine($"Notification/Email error: {ex.Message}");
            }

            TempData["Success"] = "BTEC Program rejected. Notifications sent to university representatives.";
            return Json(new { success = true, message = "Program rejected successfully." });
        }
    }
}