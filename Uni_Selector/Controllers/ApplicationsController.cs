using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.Applications;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.Student)]
    [Route("Applications")]
    public class ApplicationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public ApplicationsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: /Applications
        [HttpGet("")]
        public async Task<IActionResult> Index(string status, string searchTerm, string sortBy = "newest", int pageNumber = 1, int pageSize = 12)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                TempData["Warning"] = "Please complete your profile first.";
                return RedirectToAction("CompleteProfile", "Student");
            }

            // Get applications query
            var query = _context.StudentApplications
                .Include(a => a.UniversityProgram)
                    .ThenInclude(up => up.University)
                .Include(a => a.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(a => a.BtecProgram)
                    .ThenInclude(bp => bp.University)
                .Include(a => a.DiscountGrant)
                .Include(a => a.Commission)
                .Where(a => a.StudentId == student.Id);

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a =>
                    (a.UniversityProgram != null && a.UniversityProgram.Program.NameEnglish.Contains(searchTerm)) ||
                    (a.BtecProgram != null && a.BtecProgram.NameEnglish.Contains(searchTerm)) ||
                    (a.UniversityProgram != null && a.UniversityProgram.University.NameEnglish.Contains(searchTerm)) ||
                    (a.BtecProgram != null && a.BtecProgram.University.NameEnglish.Contains(searchTerm)) ||
                    (a.ApplicationNumber != null && a.ApplicationNumber.Contains(searchTerm)) ||
                    (a.AdmissionNumber != null && a.AdmissionNumber.Contains(searchTerm))
                );
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(a => a.ApplicationDate),
                "status" => query.OrderBy(a => a.Status).ThenByDescending(a => a.ApplicationDate),
                "university" => query.OrderBy(a => a.UniversityProgram != null ? a.UniversityProgram.University.NameEnglish : a.BtecProgram.University.NameEnglish),
                _ => query.OrderByDescending(a => a.ApplicationDate), // Default: newest
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var applications = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new ApplicationsListViewModel
            {
                Applications = applications.Select(a => new ApplicationItemViewModel
                {
                    Id = a.Id,
                    ApplicationNumber = a.ApplicationNumber,
                    AdmissionNumber = a.AdmissionNumber,
                    ProgramName = a.UniversityProgram != null ? a.UniversityProgram.Program.NameEnglish : a.BtecProgram.NameEnglish,
                    ProgramNameArabic = a.UniversityProgram != null ? a.UniversityProgram.Program.NameArabic : a.BtecProgram.NameArabic,
                    UniversityName = a.UniversityProgram != null ? a.UniversityProgram.University.NameEnglish : a.BtecProgram.University.NameEnglish,
                    UniversityLogo = a.UniversityProgram != null ? a.UniversityProgram.University.LogoPath : a.BtecProgram.University.LogoPath,
                    UniversityImage = a.UniversityProgram != null ? a.UniversityProgram.University.ImagePath : a.BtecProgram.University.ImagePath,
                    Status = a.Status,
                    ApplicationDate = a.ApplicationDate,
                    ApprovalDate = a.ApprovalDate,
                    IsBtec = a.BtecProgramId.HasValue,
                    HasDiscount = a.DiscountGrant != null,
                    DiscountCode = a.DiscountGrant?.Code,
                    CanCancel = a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.UnderReview
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                StatusFilter = status,
                SearchTerm = searchTerm,
                SortBy = sortBy
            };

            // Statistics
            ViewBag.TotalApplications = totalCount;
            ViewBag.PendingCount = await _context.StudentApplications.Where(a => a.StudentId == student.Id && a.Status == ApplicationStatus.Pending).CountAsync();
            ViewBag.ApprovedCount = await _context.StudentApplications.Where(a => a.StudentId == student.Id && a.Status == ApplicationStatus.Approved).CountAsync();
            ViewBag.RejectedCount = await _context.StudentApplications.Where(a => a.StudentId == student.Id && a.Status == ApplicationStatus.Rejected).CountAsync();

            return View(model);
        }

        // GET: /Applications/Create?recommendationId={id}
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? recommendationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null || student.ProfileCompleted != true)
            {
                TempData["Error"] = "Please complete your profile first.";
                return RedirectToAction("CompleteProfile", "Student");
            }

            var model = new CreateApplicationViewModel
            {
                StudentFullName = student.User.FullName,
                StudentEmail = student.User.Email,
                StudentGPA = student.GPA
            };

            // If coming from recommendation
            if (recommendationId.HasValue)
            {
                var recommendation = await _context.Recommendations
                    .Include(r => r.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(r => r.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(r => r.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .FirstOrDefaultAsync(r => r.Id == recommendationId && r.StudentId == student.Id);

                if (recommendation != null)
                {
                    model.RecommendationId = recommendation.Id;
                    model.UniversityProgramId = recommendation.UniversityProgramId;
                    model.BtecProgramId = recommendation.BtecProgramId;
                    model.IsBtec = recommendation.BtecProgramId.HasValue;

                    if (recommendation.UniversityProgram != null)
                    {
                        model.ProgramName = recommendation.UniversityProgram.Program.NameEnglish;
                        model.UniversityName = recommendation.UniversityProgram.University.NameEnglish;
                        model.UniversityLogo = recommendation.UniversityProgram.University.LogoPath;
                        model.EstimatedCost = recommendation.EstimatedTotalCost;
                    }
                    else if (recommendation.BtecProgram != null)
                    {
                        model.ProgramName = recommendation.BtecProgram.NameEnglish;
                        model.UniversityName = recommendation.BtecProgram.University.NameEnglish;
                        model.UniversityLogo = recommendation.BtecProgram.University.LogoPath;
                        model.EstimatedCost = recommendation.EstimatedTotalCost;
                    }

                    // Check if already applied
                    var existingApplication = await _context.StudentApplications
                        .FirstOrDefaultAsync(a => a.StudentId == student.Id &&
                            ((a.UniversityProgramId == recommendation.UniversityProgramId) ||
                             (a.BtecProgramId == recommendation.BtecProgramId)));

                    if (existingApplication != null)
                    {
                        TempData["Warning"] = "You have already applied to this program.";
                        return RedirectToAction(nameof(Details), new { id = existingApplication.Id });
                    }
                }
            }

            return View(model);
        }

        // POST: /Applications/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            // Validate only one program type is selected
            if ((model.UniversityProgramId.HasValue && model.BtecProgramId.HasValue) ||
                (!model.UniversityProgramId.HasValue && !model.BtecProgramId.HasValue))
            {
                ModelState.AddModelError("", "Please select exactly one program.");
                return View(model);
            }

            // Check for duplicate application
            var exists = await _context.StudentApplications
                .AnyAsync(a => a.StudentId == student.Id &&
                    ((a.UniversityProgramId == model.UniversityProgramId) ||
                     (a.BtecProgramId == model.BtecProgramId)));

            if (exists)
            {
                TempData["Error"] = "You have already applied to this program.";
                return RedirectToAction(nameof(Index));
            }

            // Create application
            var application = new StudentApplication
            {
                StudentId = student.Id,
                UniversityProgramId = model.UniversityProgramId,
                BtecProgramId = model.BtecProgramId,
                Status = ApplicationStatus.Pending,
                ApplicationDate = DateTime.UtcNow,
                ApplicationNumber = GenerateApplicationNumber(),
                Notes = model.Notes,
                PlannedFirstSemesterHours = model.PlannedFirstSemesterHours
            };

            _context.StudentApplications.Add(application);
            await _context.SaveChangesAsync();

            // Get university ID for notifications
            int universityId = 0;
            string programName = "";
            string universityName = "";

            if (application.UniversityProgramId.HasValue)
            {
                var program = await _context.UniversityPrograms
                    .Include(up => up.University)
                    .Include(up => up.Program)
                    .FirstOrDefaultAsync(up => up.Id == application.UniversityProgramId);

                if (program != null)
                {
                    universityId = program.UniversityId;
                    programName = program.Program.NameEnglish;
                    universityName = program.University.NameEnglish;
                }
            }
            else if (application.BtecProgramId.HasValue)
            {
                var program = await _context.BtecPrograms
                    .Include(bp => bp.University)
                    .FirstOrDefaultAsync(bp => bp.Id == application.BtecProgramId);

                if (program != null)
                {
                    universityId = program.UniversityId;
                    programName = program.NameEnglish;
                    universityName = program.University.NameEnglish;
                }
            }

            // Send notification to student
            await _notificationService.SendNotificationAsync(
                userId: user.Id,
                title: "Application Submitted Successfully",
                message: $"Your application to {programName} at {universityName} has been submitted successfully. Application Number: {application.ApplicationNumber}",
                category: NotificationCategory.ApplicationSubmitted,
                channel: NotificationChannel.InApp,
                actionUrl: $"/Applications/{application.Id}/Details"
            );

            // Send notification to university representatives
            if (universityId > 0)
            {
                await _notificationService.SendNotificationToUniversityAsync(
                    universityId: universityId,
                    title: "New Application Received",
                    message: $"A new application has been submitted by {student.User.FullName} for {programName}. Application Number: {application.ApplicationNumber}",
                    category: NotificationCategory.ApplicationSubmitted,
                    channel: NotificationChannel.InApp,
                    actionUrl: $"/UniversityRep/Applications/{application.Id}/Details"
                );
            }

            TempData["Success"] = $"Application submitted successfully! Your application number is: {application.ApplicationNumber}";
            return RedirectToAction(nameof(Details), new { id = application.Id });
        }

        // GET: /Applications/{id}/Details
        [HttpGet("{id:int}/Details")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var application = await _context.StudentApplications
                    .Include(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .Include(a => a.HourDiscountSetByUser)
                    .Include(a => a.DiscountGrant)
                    .Include(a => a.Commission)
                    .FirstOrDefaultAsync(a => a.Id == id);



                if (application == null)
                {
                    TempData["Error"] = "Application not found.";
                    return RedirectToAction(nameof(Index));
                }

                var isBtec = application.BtecProgramId != null;

                // Get university details
                var university = isBtec
                    ? application.BtecProgram.University
                    : application.UniversityProgram.University;

                // Get program details
                var programNameEn = isBtec
                    ? application.BtecProgram.NameEnglish
                    : application.UniversityProgram.Program.NameEnglish;

                var programNameAr = isBtec
                    ? application.BtecProgram.NameArabic
                    : application.UniversityProgram.Program.NameArabic;

                var programDescription = isBtec
                    ? application.BtecProgram.Description
                    : application.UniversityProgram.Program.Description;

                var degree = isBtec
                    ? "BTEC " + application.BtecProgram.Level.ToString()
                    : application.UniversityProgram.Program.Degree.ToString();

                var btecLevel = isBtec ? application.BtecProgram.Level.ToString() : null;

                var language = isBtec
                    ? application.BtecProgram.Language.ToString()
                    : application.UniversityProgram.Program.Language.ToString();

                var studySystem = isBtec
                    ? "Morning"
                    : application.UniversityProgram.StudySystem.ToString();

                var durationYears = isBtec
                    ? application.BtecProgram.DurationInYears
                    : application.UniversityProgram.DurationInYears;

                var totalCreditHours = isBtec
                    ? application.BtecProgram.TotalCreditHours
                    : application.UniversityProgram.Program.TotalCreditHours;

                var hourPriceBase = isBtec
                    ? application.BtecProgram.HourPriceBase
                    : application.UniversityProgram.HourPriceBase;

                var registrationFee = isBtec
                    ? application.BtecProgram.RegistrationFeeFirstSemester
                    : application.UniversityProgram.RegistrationFeeFirstSemester;

                var technicalField = isBtec ? application.BtecProgram.TechnicalField : null;

                var semesterStart = isBtec
                    ? application.BtecProgram.SemesterStartDate
                    : (DateTime?)null;

                // Calculate discount details if exists
                decimal? discountPercentage = null;
                decimal? discountAmount = null;
                if (application.DiscountGrant != null)
                {
                    discountPercentage = application.DiscountGrant.Percentage;
                    discountAmount = application.DiscountGrant.AmountEstimated;
                }

                var viewModel = new ApplicationDetailsViewModel
                {
                    Id = application.Id,
                    ApplicationNumber = application.ApplicationNumber ?? $"APP-{application.Id:D6}",
                    Status = application.Status,
                    StatusText = application.Status.ToString(),
                    ApplicationDate = application.ApplicationDate,
                    ApprovalDate = application.ApprovalDate,
                    UpdatedAt = application.UpdatedAt,
                    Notes = application.Notes,
                    AdmissionNumber = application.AdmissionNumber,
                    RejectionReason = application.RejectionReason,

                    // Student info
                    StudentId = application.StudentId,
                    StudentName = application.Student.User.FullName,
                    StudentFullName = application.Student.User.FullName,
                    StudentEmail = application.Student.User.Email,
                    StudentPhone = application.Student.User.PhoneNumber ?? "N/A",
                    StudentGPA = application.Student.GPA,
                    StudentProvince = application.Student.Province,
                    StudentCity = application.Student.City,
                    StudentPath = application.Student.Path,
                    RegistrationBudget = application.Student.RegistrationBudget,

                    // University/Program info
                    IsBtec = isBtec,
                    UniversityName = university.NameEnglish,
                    UniversityLogo = university.LogoPath,
                    UniversityAddress = university.FullAddress,
                    UniversityPhone = university.PhoneNumber,
                    UniversityEmail = university.Email,
                    UniversityWebsite = university.OfficialWebsite,

                    ProgramNameEnglish = programNameEn,
                    ProgramName = programNameEn,
                    ProgramNameArabic = programNameAr,
                    ProgramDescription = programDescription,
                    Degree = degree,
                    BtecLevel = btecLevel,
                    Language = language,
                    StudySystem = studySystem,
                    DurationInYears = durationYears,
                    DurationYears = durationYears,
                    TotalCreditHours = totalCreditHours,
                    TechnicalField = technicalField,
                    SemesterStartDate = semesterStart,

                    // Financial info
                    HourPriceBase = hourPriceBase,
                    RegistrationFeeFirstSemester = registrationFee,
                    HourDiscountPercent = application.HourDiscountPercent,
                    HourDiscountSetBy = application.HourDiscountSetByUser?.FullName,
                    HourDiscountSetAt = application.HourDiscountSetAt,
                    PlannedFirstSemesterHours = application.PlannedFirstSemesterHours,

                    // Discount grant
                    HasDiscountGrant = application.DiscountGrant != null,
                    DiscountCode = application.DiscountGrant?.Code,
                    DiscountStatus = application.DiscountGrant?.Status,
                    DiscountGrantedAt = application.DiscountGrant?.GrantedAt,
                    DiscountRedeemedAt = application.DiscountGrant?.RedeemedAt,
                    DiscountPercentage = discountPercentage,
                    DiscountAmount = discountAmount,

                    // Commission
                    HasCommission = application.Commission != null,
                    CommissionAmount = application.Commission?.AmountEstimated,
                    CommissionSettled = application.Commission?.Settled
                };

                // Calculate estimated cost
                if (application.PlannedFirstSemesterHours.HasValue)
                {
                    var hourPrice = hourPriceBase;
                    if (application.HourDiscountPercent.HasValue)
                    {
                        hourPrice = hourPrice * (1 - application.HourDiscountPercent.Value / 100);
                    }
                    viewModel.EstimatedTotalCost = registrationFee + (hourPrice * application.PlannedFirstSemesterHours.Value);

                    // Subtract platform discount if exists
                    if (discountAmount.HasValue)
                    {
                        viewModel.EstimatedTotalCost -= discountAmount.Value;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading application details.";
                return RedirectToAction(nameof(Index));
            }
        }
        // POST: /Applications/{id}/Cancel
        [HttpPost("{id:int}/Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return Json(new { success = false, message = "Student not found" });

            var application = await _context.StudentApplications
                .Include(a => a.DiscountGrant)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student.Id);

            if (application == null)
                return Json(new { success = false, message = "Application not found" });

            // Can only cancel pending or under review applications
            if (application.Status != ApplicationStatus.Pending && application.Status != ApplicationStatus.UnderReview)
            {
                return Json(new { success = false, message = "Cannot cancel this application" });
            }

            application.Status = ApplicationStatus.Cancelled;
            application.UpdatedAt = DateTime.UtcNow;

            // Expire discount if exists
            if (application.DiscountGrant != null && application.DiscountGrant.Status == DiscountStatus.Issued)
            {
                application.DiscountGrant.Status = DiscountStatus.Expired;
            }

            await _context.SaveChangesAsync();

            // Get program and university details for notification
            string programName = "";
            string universityName = "";
            int universityId = 0;

            if (application.UniversityProgramId.HasValue)
            {
                var program = await _context.UniversityPrograms
                    .Include(up => up.University)
                    .Include(up => up.Program)
                    .FirstOrDefaultAsync(up => up.Id == application.UniversityProgramId);

                if (program != null)
                {
                    programName = program.Program.NameEnglish;
                    universityName = program.University.NameEnglish;
                    universityId = program.UniversityId;
                }
            }
            else if (application.BtecProgramId.HasValue)
            {
                var program = await _context.BtecPrograms
                    .Include(bp => bp.University)
                    .FirstOrDefaultAsync(bp => bp.Id == application.BtecProgramId);

                if (program != null)
                {
                    programName = program.NameEnglish;
                    universityName = program.University.NameEnglish;
                    universityId = program.UniversityId;
                }
            }

            // Send notification to student
            await _notificationService.SendNotificationAsync(
                userId: user.Id,
                title: "Application Cancelled",
                message: $"Your application to {programName} at {universityName} (Application #{application.ApplicationNumber}) has been cancelled.",
                category: NotificationCategory.SystemAlert,
                channel: NotificationChannel.InApp,
                actionUrl: $"/Applications/{application.Id}/Details"
            );

            // Send notification to university
            if (universityId > 0)
            {
                await _notificationService.SendNotificationToUniversityAsync(
                    universityId: universityId,
                    title: "Application Cancelled",
                    message: $"Application #{application.ApplicationNumber} for {programName} has been cancelled by the student.",
                    category: NotificationCategory.SystemAlert,
                    channel: NotificationChannel.InApp,
                    actionUrl: $"/UniversityRep/Applications/{application.Id}/Details"
                );
            }

            return Json(new { success = true, message = "Application cancelled successfully" });
        }

        #region Helper Methods

        private string GenerateApplicationNumber()
        {
            // Format: APP-YYYYMMDD-XXXX
            var date = DateTime.UtcNow;
            var random = new Random();
            var number = random.Next(1000, 9999);
            return $"APP-{date:yyyyMMdd}-{number}";
        }

        #endregion
    }
}
