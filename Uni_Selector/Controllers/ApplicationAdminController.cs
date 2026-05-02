using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.AdminApplication;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.PlatformAdmin)]
    public class ApplicationAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApplicationAdminController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationAdminController(
            AppDbContext context,
            ILogger<ApplicationAdminController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            ApplicationStatus? status = null,
            int? universityId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _context.StudentApplications
                    .Include(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(a =>
                        a.ApplicationNumber.Contains(searchTerm) ||
                        a.Student.User.FullName.Contains(searchTerm) ||
                        a.Student.User.Email.Contains(searchTerm));
                }

                // Apply status filter
                if (status.HasValue)
                {
                    query = query.Where(a => a.Status == status.Value);
                }

                // Apply university filter
                if (universityId.HasValue)
                {
                    query = query.Where(a =>
                        (a.UniversityProgram != null && a.UniversityProgram.UniversityId == universityId.Value) ||
                        (a.BtecProgram != null && a.BtecProgram.UniversityId == universityId.Value));
                }

                // Apply date range filter
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.ApplicationDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.ApplicationDate <= endDate.Value.AddDays(1));
                }

                var totalApplications = await query.CountAsync();

                var applications = await query
                    .OrderByDescending(a => a.ApplicationDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ApplicationListItemViewModel
                    {
                        Id = a.Id,
                        ApplicationNumber = a.ApplicationNumber ?? $"APP-{a.Id:D6}",
                        StudentName = a.Student.User.FullName,
                        StudentEmail = a.Student.User.Email,
                        UniversityName = a.UniversityProgram != null
                            ? a.UniversityProgram.University.NameEnglish
                            : a.BtecProgram.University.NameEnglish,
                        ProgramName = a.UniversityProgram != null
                            ? a.UniversityProgram.Program.NameEnglish
                            : a.BtecProgram.NameEnglish,
                        Degree = a.UniversityProgram != null
                            ? a.UniversityProgram.Program.Degree.ToString()
                            : a.BtecProgram.Level.ToString(),
                        Status = a.Status,
                        StatusText = a.Status.ToString(),
                        ApplicationDate = a.ApplicationDate,
                        ApprovalDate = a.ApprovalDate,
                        IsBtec = a.BtecProgramId != null
                    })
                    .ToListAsync();

                // Set badge classes
                foreach (var app in applications)
                {
                    app.StatusBadgeClass = app.GetStatusBadgeClass();
                }

                // Get universities for filter
                var universities = await _context.Universities
                    .Where(u => u.IsActive)
                    .Select(u => new UniversityFilterOption
                    {
                        Id = u.Id,
                        Name = u.NameEnglish
                    })
                    .OrderBy(u => u.Name)
                    .ToListAsync();

                var viewModel = new ApplicationListViewModel
                {
                    Applications = applications,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalApplications = totalApplications,
                    SearchTerm = searchTerm,
                    Status = status,
                    UniversityId = universityId,
                    StartDate = startDate,
                    EndDate = endDate,
                    AllStatuses = Enum.GetValues(typeof(ApplicationStatus)).Cast<ApplicationStatus>().ToList(),
                    Universities = universities
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading applications list");
                TempData["Error"] = "An error occurred while loading applications.";
                return View(new ApplicationListViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var now = DateTime.UtcNow;
                var twelveMonthsAgo = now.AddMonths(-12);

                var allApplications = await _context.StudentApplications
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .Include(a => a.Student)
                        .ThenInclude(s => s.User)
                    .ToListAsync();

                var totalApplications = allApplications.Count;

                // Summary cards
                var pendingCount = allApplications.Count(a => a.Status == ApplicationStatus.Pending);
                var approvedCount = allApplications.Count(a => a.Status == ApplicationStatus.Approved);
                var rejectedCount = allApplications.Count(a => a.Status == ApplicationStatus.Rejected);
                var enrolledCount = allApplications.Count(a => a.Status == ApplicationStatus.Enrolled);

                // Status distribution
                var statusDistribution = allApplications
                    .GroupBy(a => a.Status)
                    .Select(g => new StatusDistribution
                    {
                        Status = g.Key,
                        StatusName = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = totalApplications > 0 ? Math.Round((decimal)g.Count() / totalApplications * 100, 1) : 0
                    })
                    .ToList();

                foreach (var dist in statusDistribution)
                {
                    dist.Color = dist.GetColor();
                }

                // Monthly applications (last 12 months)
                var monthlyApplications = Enumerable.Range(0, 12)
                    .Select(i => now.AddMonths(-i))
                    .Select(date => new MonthlyApplicationCount
                    {
                        Year = date.Year,
                        Month = date.Month,
                        MonthName = date.ToString("MMM yyyy"),
                        Count = allApplications.Count(a =>
                            a.ApplicationDate.Year == date.Year &&
                            a.ApplicationDate.Month == date.Month)
                    })
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .ToList();

                // Top universities
                var topUniversities = allApplications
                    .GroupBy(a => new
                    {
                        UniversityId = a.UniversityProgram != null
                            ? a.UniversityProgram.UniversityId
                            : a.BtecProgram.UniversityId,
                        UniversityName = a.UniversityProgram != null
                            ? a.UniversityProgram.University.NameEnglish
                            : a.BtecProgram.University.NameEnglish
                    })
                    .Select(g => new UniversityApplicationCount
                    {
                        UniversityId = g.Key.UniversityId,
                        UniversityName = g.Key.UniversityName,
                        ApplicationCount = g.Count(),
                        ApprovedCount = g.Count(a => a.Status == ApplicationStatus.Approved),
                        PendingCount = g.Count(a => a.Status == ApplicationStatus.Pending)
                    })
                    .OrderByDescending(u => u.ApplicationCount)
                    .Take(10)
                    .ToList();

                // Top programs
                var topPrograms = allApplications
                    .GroupBy(a => new
                    {
                        ProgramName = a.UniversityProgram != null
                            ? a.UniversityProgram.Program.NameEnglish
                            : a.BtecProgram.NameEnglish,
                        Degree = a.UniversityProgram != null
                            ? a.UniversityProgram.Program.Degree.ToString()
                            : "BTEC " + a.BtecProgram.Level.ToString()
                    })
                    .Select(g => new ProgramApplicationCount
                    {
                        ProgramName = g.Key.ProgramName,
                        Degree = g.Key.Degree,
                        ApplicationCount = g.Count()
                    })
                    .OrderByDescending(p => p.ApplicationCount)
                    .Take(10)
                    .ToList();

                // Recent activity
                var recentActivity = allApplications
                    .OrderByDescending(a => a.UpdatedAt ?? a.ApplicationDate)
                    .Take(10)
                    .Select(a => new RecentApplicationActivity
                    {
                        ApplicationNumber = a.ApplicationNumber ?? $"APP-{a.Id:D6}",
                        StudentName = a.Student.User.FullName,
                        UniversityName = a.UniversityProgram != null
                            ? a.UniversityProgram.University.NameEnglish
                            : a.BtecProgram.University.NameEnglish,
                        Status = a.Status,
                        StatusText = a.Status.ToString(),
                        ActivityDate = a.UpdatedAt ?? a.ApplicationDate,
                        ActivityType = a.UpdatedAt != null ? "Status Update" : "New Application"
                    })
                    .ToList();

                // Calculate metrics
                var approvalRate = totalApplications > 0
                    ? Math.Round((decimal)approvedCount / totalApplications * 100, 1)
                    : 0;
                var enrollmentRate = totalApplications > 0
                    ? Math.Round((decimal)enrolledCount / totalApplications * 100, 1)
                    : 0;
                var rejectionRate = totalApplications > 0
                    ? Math.Round((decimal)rejectedCount / totalApplications * 100, 1)
                    : 0;

                // Average processing time
                var processedApplications = allApplications.Where(a => a.ApprovalDate != null).ToList();
                var averageProcessingDays = processedApplications.Any()
                    ? processedApplications.Average(a => (a.ApprovalDate!.Value - a.ApplicationDate).TotalDays)
                    : 0;

                var viewModel = new ApplicationStatisticsViewModel
                {
                    TotalApplications = totalApplications,
                    PendingApplications = pendingCount,
                    ApprovedApplications = approvedCount,
                    RejectedApplications = rejectedCount,
                    EnrolledApplications = enrolledCount,
                    ApprovalRate = approvalRate,
                    EnrollmentRate = enrollmentRate,
                    RejectionRate = rejectionRate,
                    StatusDistribution = statusDistribution,
                    MonthlyApplications = monthlyApplications,
                    TopUniversities = topUniversities,
                    TopPrograms = topPrograms,
                    RegularApplications = allApplications.Count(a => a.UniversityProgramId != null),
                    BtecApplications = allApplications.Count(a => a.BtecProgramId != null),
                    RecentActivity = recentActivity,
                    AverageProcessingDays = Math.Round(averageProcessingDays, 1)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading application statistics");
                TempData["Error"] = "An error occurred while loading statistics.";
                return View(new ApplicationStatisticsViewModel());
            }
        }

        [HttpGet]
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
                    StudentEmail = application.Student.User.Email,
                    StudentPhone = application.Student.User.PhoneNumber ?? "N/A",
                    StudentGPA = application.Student.GPA,
                    StudentProvince = application.Student.Province,
                    StudentCity = application.Student.City,
                    StudentPath = application.Student.Path,
                    RegistrationBudget = application.Student.RegistrationBudget,

                    // University/Program info
                    IsBtec = isBtec,
                    UniversityName = isBtec
                        ? application.BtecProgram.University.NameEnglish
                        : application.UniversityProgram.University.NameEnglish,
                    ProgramNameEnglish = isBtec
                        ? application.BtecProgram.NameEnglish
                        : application.UniversityProgram.Program.NameEnglish,
                    ProgramNameArabic = isBtec
                        ? application.BtecProgram.NameArabic
                        : application.UniversityProgram.Program.NameArabic,
                    Degree = isBtec
                        ? "BTEC " + application.BtecProgram.Level.ToString()
                        : application.UniversityProgram.Program.Degree.ToString(),
                    Language = isBtec
                        ? application.BtecProgram.Language.ToString()
                        : application.UniversityProgram.Program.Language.ToString(),
                    StudySystem = isBtec
                        ? "Morning"
                        : application.UniversityProgram.StudySystem.ToString(),
                    DurationInYears = isBtec
                        ? application.BtecProgram.DurationInYears
                        : application.UniversityProgram.DurationInYears,
                    TotalCreditHours = isBtec
                        ? application.BtecProgram.TotalCreditHours
                        : application.UniversityProgram.Program.TotalCreditHours,

                    // Financial info
                    HourPriceBase = isBtec
                        ? application.BtecProgram.HourPriceBase
                        : application.UniversityProgram.HourPriceBase,
                    RegistrationFeeFirstSemester = isBtec
                        ? application.BtecProgram.RegistrationFeeFirstSemester
                        : application.UniversityProgram.RegistrationFeeFirstSemester,
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

                    // Commission
                    HasCommission = application.Commission != null,
                    CommissionAmount = application.Commission?.AmountEstimated,
                    CommissionSettled = application.Commission?.Settled
                };

                // Calculate estimated cost
                if (application.PlannedFirstSemesterHours.HasValue)
                {
                    var hourPrice = viewModel.HourPriceBase;
                    if (application.HourDiscountPercent.HasValue)
                    {
                        hourPrice = hourPrice * (1 - application.HourDiscountPercent.Value / 100);
                    }
                    viewModel.EstimatedTotalCost = viewModel.RegistrationFeeFirstSemester +
                        (hourPrice * application.PlannedFirstSemesterHours.Value);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading application details for ID: {id}");
                TempData["Error"] = "An error occurred while loading application details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Override(int id)
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
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    TempData["Error"] = "Application not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ApplicationOverrideViewModel
                {
                    ApplicationId = application.Id,
                    ApplicationNumber = application.ApplicationNumber ?? $"APP-{application.Id:D6}",
                    StudentName = application.Student.User.FullName,
                    UniversityName = application.UniversityProgram != null
                        ? application.UniversityProgram.University.NameEnglish
                        : application.BtecProgram.University.NameEnglish,
                    ProgramName = application.UniversityProgram != null
                        ? application.UniversityProgram.Program.NameEnglish
                        : application.BtecProgram.NameEnglish,
                    CurrentStatus = application.Status,
                    AdmissionNumber = application.AdmissionNumber,
                    RejectionReason = application.RejectionReason
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading override form for application ID: {id}");
                TempData["Error"] = "An error occurred while loading the override form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Override(int id, ApplicationOverrideViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var application = await _context.StudentApplications.FindAsync(id);
                if (application == null)
                {
                    TempData["Error"] = "Application not found.";
                    return RedirectToAction(nameof(Index));
                }

                var oldStatus = application.Status;
                application.Status = model.NewStatus;
                application.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(model.OverrideNotes))
                {
                    application.Notes = string.IsNullOrEmpty(application.Notes)
                        ? $"[ADMIN OVERRIDE] {model.OverrideNotes}"
                        : $"{application.Notes}\n\n[ADMIN OVERRIDE] {model.OverrideNotes}";
                }

                if (model.NewStatus == ApplicationStatus.Approved)
                {
                    application.ApprovalDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(model.AdmissionNumber))
                    {
                        application.AdmissionNumber = model.AdmissionNumber;
                    }
                    application.RejectionReason = null;
                }
                else if (model.NewStatus == ApplicationStatus.Rejected)
                {
                    if (!string.IsNullOrEmpty(model.RejectionReason))
                    {
                        application.RejectionReason = model.RejectionReason;
                    }
                    // Clear approval-owned fields to avoid stale data
                    application.ApprovalDate = null;
                    application.AdmissionNumber = null;
                }
                else
                {
                    // For any other status (Pending, UnderReview, Enrolled, etc.)
                    // clear both approval and rejection stale fields
                    application.ApprovalDate = null;
                    application.RejectionReason = null;
                    application.AdmissionNumber = null;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Application {application.ApplicationNumber} status overridden from {oldStatus} to {model.NewStatus} by admin");

                TempData["Success"] = $"Application status successfully updated from {oldStatus} to {model.NewStatus}.";
                return RedirectToAction(nameof(Details), new { id = application.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error overriding application ID: {id}");
                ModelState.AddModelError("", "An error occurred while updating the application status.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            try
            {
                var applications = await _context.StudentApplications
                    .Include(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .OrderByDescending(a => a.ApplicationDate)
                    .ToListAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Applications");

                // Headers
                worksheet.Cell(1, 1).Value = "Application Number";
                worksheet.Cell(1, 2).Value = "Student Name";
                worksheet.Cell(1, 3).Value = "Student Email";
                worksheet.Cell(1, 4).Value = "Student GPA";
                worksheet.Cell(1, 5).Value = "University";
                worksheet.Cell(1, 6).Value = "Program";
                worksheet.Cell(1, 7).Value = "Degree";
                worksheet.Cell(1, 8).Value = "Type";
                worksheet.Cell(1, 9).Value = "Status";
                worksheet.Cell(1, 10).Value = "Application Date";
                worksheet.Cell(1, 11).Value = "Approval Date";
                worksheet.Cell(1, 12).Value = "Admission Number";
                worksheet.Cell(1, 13).Value = "Rejection Reason";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 13);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int row = 2;
                foreach (var app in applications)
                {
                    worksheet.Cell(row, 1).Value = app.ApplicationNumber ?? $"APP-{app.Id:D6}";
                    worksheet.Cell(row, 2).Value = app.Student.User.FullName;
                    worksheet.Cell(row, 3).Value = app.Student.User.Email;
                    worksheet.Cell(row, 4).Value = app.Student.GPA;
                    worksheet.Cell(row, 5).Value = app.UniversityProgram != null
                        ? app.UniversityProgram.University.NameEnglish
                        : app.BtecProgram.University.NameEnglish;
                    worksheet.Cell(row, 6).Value = app.UniversityProgram != null
                        ? app.UniversityProgram.Program.NameEnglish
                        : app.BtecProgram.NameEnglish;
                    worksheet.Cell(row, 7).Value = app.UniversityProgram != null
                        ? app.UniversityProgram.Program.Degree.ToString()
                        : "BTEC " + app.BtecProgram.Level.ToString();
                    worksheet.Cell(row, 8).Value = app.BtecProgramId != null ? "BTEC" : "Regular";
                    worksheet.Cell(row, 9).Value = app.Status.ToString();
                    worksheet.Cell(row, 10).Value = app.ApplicationDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 11).Value = app.ApprovalDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
                    worksheet.Cell(row, 12).Value = app.AdmissionNumber ?? "";
                    worksheet.Cell(row, 13).Value = app.RejectionReason ?? "";
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = $"Applications_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting applications");
                TempData["Error"] = "An error occurred while exporting applications.";
                return RedirectToAction(nameof(Index));
            }
        }
    }

}
