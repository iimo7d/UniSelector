using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.UniversityReps;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "UniversityRep")]
    [Route("UniversityRep")]
    public class UniversityRepController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UniversityRepController> _logger;

        public UniversityRepController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<UniversityRepController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /UniversityRep/Dashboard
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var rep = await _context.UniversityRepresentatives
                    .Include(r => r.University)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

                if (rep == null)
                {
                    _logger.LogWarning($"No active university representative found for user {userId}");
                    TempData["Error"] = "You are not assigned to any university.";
                    return RedirectToAction("Index", "Home");
                }

                var universityId = rep.UniversityId;

                // Get applications using JOINs
                var applications = await _context.StudentApplications
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .Where(a => (a.UniversityProgram != null && a.UniversityProgram.UniversityId == universityId) ||
                                (a.BtecProgram != null && a.BtecProgram.UniversityId == universityId))
                    .ToListAsync();

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var applicationsThisMonth = applications.Where(a => a.ApplicationDate >= thirtyDaysAgo).ToList();

                // Get programs statistics
                var activePrograms = await _context.UniversityPrograms
                    .Where(p => p.UniversityId == universityId && p.IsActive)
                    .CountAsync();

                var activeBtecPrograms = await _context.BtecPrograms
                    .Where(p => p.UniversityId == universityId && p.IsActive && p.IsApprovedByBtecAuthority)
                    .CountAsync();

                // Get commission statistics
                var commissions = await _context.Commissions
                    .Where(c => c.UniversityId == universityId)
                    .ToListAsync();

                var totalCommissionEarned = commissions.Sum(c => c.AmountEstimated);
                var pendingSettlement = commissions.Where(c => !c.Settled).Sum(c => c.AmountEstimated);

                // Get recent applications (last 10) - FIXED with proper null checks
                var recentApplications = await _context.StudentApplications
                    .Include(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(a => a.BtecProgram)
                    .Where(a => (a.UniversityProgram != null && a.UniversityProgram.UniversityId == universityId) ||
                                (a.BtecProgram != null && a.BtecProgram.UniversityId == universityId))
                    .OrderByDescending(a => a.ApplicationDate)
                    .Take(10)
                    .ToListAsync();

                var recentApplicationViewModels = recentApplications.Select(a => new RecentApplicationViewModel
                {
                    Id = a.Id,
                    ApplicationNumber = a.ApplicationNumber ?? "N/A",
                    StudentName = a.Student?.User?.FullName ?? "Unknown",
                    StudentEmail = a.Student?.User?.Email ?? "N/A",
                    ProgramName = a.UniversityProgram?.Program?.NameEnglish ?? a.BtecProgram?.NameEnglish ?? "N/A",
                    ProgramType = a.UniversityProgram != null ? "University Program" : "BTEC Program",
                    Status = a.Status,
                    ApplicationDate = a.ApplicationDate
                }).ToList();

                // Application status breakdown
                var statusBreakdown = applications
                    .GroupBy(a => a.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());

                // Monthly applications trend (last 6 months)
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var monthlyTrend = applications
                    .Where(a => a.ApplicationDate >= sixMonthsAgo)
                    .GroupBy(a => new { a.ApplicationDate.Year, a.ApplicationDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new MonthlyStatViewModel
                    {
                        Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                        Count = g.Count()
                    })
                    .ToList();

                var viewModel = new UniversityRepDashboardViewModel
                {
                    UniversityName = rep.University?.NameEnglish ?? "Unknown University",
                    RepresentativeName = rep.User?.FullName ?? "Unknown Representative",

                    // KPI Cards
                    TotalApplications = applications.Count,
                    PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
                    ApprovedApplications = applications.Count(a => a.Status == ApplicationStatus.Approved),
                    RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected),

                    ApplicationsThisMonth = applicationsThisMonth.Count,
                    ApplicationsLastMonth = applications.Where(a => a.ApplicationDate >= DateTime.UtcNow.AddDays(-60) && a.ApplicationDate < thirtyDaysAgo).Count(),

                    ActivePrograms = activePrograms,
                    ActiveBtecPrograms = activeBtecPrograms,

                    TotalCommissionEarned = totalCommissionEarned,
                    PendingSettlement = pendingSettlement,

                    // Charts data
                    ApplicationStatusBreakdown = statusBreakdown,
                    MonthlyApplicationsTrend = monthlyTrend,

                    // Recent activity
                    RecentApplications = recentApplicationViewModels
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading university rep dashboard");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return View(new UniversityRepDashboardViewModel
                {
                    UniversityName = "Unknown",
                    RepresentativeName = "Unknown"
                });
            }
        }
        // GET: /UniversityRep/Analytics/Applications
        [HttpGet("Analytics/Applications")]
        public async Task<IActionResult> ApplicationAnalytics(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var rep = await _context.UniversityRepresentatives
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

                if (rep == null)
                {
                    TempData["Error"] = "You are not assigned to any university.";
                    return RedirectToAction("Dashboard");
                }

                var universityId = rep.UniversityId;

                // Default to last 90 days
                var end = endDate ?? DateTime.UtcNow;
                var start = startDate ?? end.AddDays(-90);

                var applications = await _context.StudentApplications
                    .Include(a => a.Student)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(a => a.BtecProgram)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .Where(a => ((a.UniversityProgram != null && a.UniversityProgram.UniversityId == universityId) ||
                                (a.BtecProgram != null && a.BtecProgram.UniversityId == universityId)) &&
                                a.ApplicationDate >= start &&
                                a.ApplicationDate <= end)
                    .ToListAsync();

                // Status distribution
                var statusDistribution = applications
                    .GroupBy(a => a.Status)
                    .Select(g => new StatusDistributionViewModel
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = applications.Count > 0 ? (decimal)g.Count() / applications.Count * 100 : 0
                    })
                    .ToList();

                // Daily applications (last 30 days)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var dailyApplications = applications
                    .Where(a => a.ApplicationDate >= thirtyDaysAgo)
                    .GroupBy(a => a.ApplicationDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DailyStatViewModel
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                // Program-wise applications
                var programStats = applications
                    .GroupBy(a => a.UniversityProgram != null ?
                        a.UniversityProgram.Program.NameEnglish :
                        a.BtecProgram.NameEnglish)
                    .Select(g => new ProgramStatViewModel
                    {
                        ProgramName = g.Key,
                        TotalApplications = g.Count(),
                        Approved = g.Count(a => a.Status == ApplicationStatus.Approved),
                        Rejected = g.Count(a => a.Status == ApplicationStatus.Rejected),
                        Pending = g.Count(a => a.Status == ApplicationStatus.Pending),
                        ApprovalRate = g.Count() > 0 ?
                            (decimal)g.Count(a => a.Status == ApplicationStatus.Approved) / g.Count() * 100 : 0
                    })
                    .OrderByDescending(p => p.TotalApplications)
                    .ToList();

                // Average processing time
                var processedApplications = applications
                    .Where(a => a.ApprovalDate.HasValue && a.Status != ApplicationStatus.Pending)
                    .ToList();

                var avgProcessingTime = processedApplications.Any() ?
                    processedApplications.Average(a => (a.ApprovalDate.Value - a.ApplicationDate).TotalDays) : 0;

                var viewModel = new ApplicationAnalyticsViewModel
                {
                    StartDate = start,
                    EndDate = end,
                    TotalApplications = applications.Count,
                    AverageProcessingTimeDays = avgProcessingTime,
                    StatusDistribution = statusDistribution,
                    DailyApplications = dailyApplications,
                    ProgramStatistics = programStats
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading application analytics");
                TempData["Error"] = "An error occurred while loading analytics.";
                return View(new ApplicationAnalyticsViewModel());
            }
        }

        // GET: /UniversityRep/Analytics/Programs
        [HttpGet("Analytics/Programs")]
        public async Task<IActionResult> ProgramAnalytics()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var rep = await _context.UniversityRepresentatives
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

                if (rep == null)
                {
                    TempData["Error"] = "You are not assigned to any university.";
                    return RedirectToAction("Dashboard");
                }

                var universityId = rep.UniversityId;

                // University Programs
                var universityPrograms = await _context.UniversityPrograms
                    .Where(p => p.UniversityId == universityId)
                    .Include(p => p.Program)
                    .ToListAsync();

                var programStats = new List<ProgramPerformanceViewModel>();

                foreach (var program in universityPrograms)
                {
                    var applications = await _context.StudentApplications
                        .Where(a => a.UniversityProgramId == program.Id)
                        .ToListAsync();

                    var recommendations = await _context.Recommendations
                        .Where(r => r.UniversityProgramId == program.Id)
                        .ToListAsync();

                    programStats.Add(new ProgramPerformanceViewModel
                    {
                        ProgramId = program.Id,
                        ProgramName = program.Program.NameEnglish,
                        ProgramType = "University Program",
                        Degree = program.Program.Degree.ToString(),
                        Language = program.Program.Language.ToString(),
                        TotalApplications = applications.Count,
                        ApprovedApplications = applications.Count(a => a.Status == ApplicationStatus.Approved),
                        PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
                        RecommendationCount = recommendations.Count,
                        ConversionRate = recommendations.Count > 0 ?
                            (decimal)applications.Count / recommendations.Count * 100 : 0,
                        Capacity = program.Capacity,
                        CapacityUtilization = program.Capacity > 0 ?
                            (decimal)applications.Count(a => a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Enrolled) / program.Capacity * 100 : 0,
                        IsActive = program.IsActive,
                        HourPriceBase = program.HourPriceBase,
                        RegistrationFee = program.RegistrationFeeFirstSemester
                    });
                }

                // BTEC Programs
                var btecPrograms = await _context.BtecPrograms
                    .Where(p => p.UniversityId == universityId)
                    .ToListAsync();

                foreach (var program in btecPrograms)
                {
                    var applications = await _context.StudentApplications
                        .Where(a => a.BtecProgramId == program.Id)
                        .ToListAsync();

                    var recommendations = await _context.Recommendations
                        .Where(r => r.BtecProgramId == program.Id)
                        .ToListAsync();

                    programStats.Add(new ProgramPerformanceViewModel
                    {
                        ProgramId = program.Id,
                        ProgramName = program.NameEnglish,
                        ProgramType = "BTEC Program",
                        Degree = program.Level.ToString(),
                        Language = program.Language.ToString(),
                        TotalApplications = applications.Count,
                        ApprovedApplications = applications.Count(a => a.Status == ApplicationStatus.Approved),
                        PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
                        RecommendationCount = recommendations.Count,
                        ConversionRate = recommendations.Count > 0 ?
                            (decimal)applications.Count / recommendations.Count * 100 : 0,
                        Capacity = program.Capacity,
                        CapacityUtilization = program.Capacity > 0 ?
                            (decimal)applications.Count(a => a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Enrolled) / program.Capacity * 100 : 0,
                        IsActive = program.IsActive,
                        HourPriceBase = program.HourPriceBase,
                        RegistrationFee = program.RegistrationFeeFirstSemester
                    });
                }

                // Degree distribution
                var degreeDistribution = programStats
                    .GroupBy(p => p.Degree)
                    .Select(g => new { Degree = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.Degree, x => x.Count);

                // Language distribution
                var languageDistribution = programStats
                    .GroupBy(p => p.Language)
                    .Select(g => new { Language = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.Language, x => x.Count);

                var viewModel = new ProgramAnalyticsViewModel
                {
                    Programs = programStats.OrderByDescending(p => p.TotalApplications).ToList(),
                    TotalPrograms = programStats.Count,
                    ActivePrograms = programStats.Count(p => p.IsActive),
                    TotalApplications = programStats.Sum(p => p.TotalApplications),
                    AverageConversionRate = programStats.Any() ? programStats.Average(p => p.ConversionRate) : 0,
                    DegreeDistribution = degreeDistribution,
                    LanguageDistribution = languageDistribution
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program analytics");
                TempData["Error"] = "An error occurred while loading analytics.";
                return View(new ProgramAnalyticsViewModel());
            }
        }

        // GET: /UniversityRep/Analytics/Demographics
        [HttpGet("Analytics/Demographics")]
        public async Task<IActionResult> Demographics()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var rep = await _context.UniversityRepresentatives
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

                if (rep == null)
                {
                    TempData["Error"] = "You are not assigned to any university.";
                    return RedirectToAction("Dashboard");
                }

                var universityId = rep.UniversityId;

                var students = await _context.StudentApplications
                    .Include(a => a.Student)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .Where(a => (a.UniversityProgram != null && a.UniversityProgram.UniversityId == universityId) ||
                                (a.BtecProgram != null && a.BtecProgram.UniversityId == universityId))
                    .Select(a => a.Student)
                    .Distinct()
                    .ToListAsync();

                // Gender distribution
                var genderDistribution = students
                    .GroupBy(s => s.Gender)
                    .Select(g => new { Gender = g.Key.ToString(), Count = g.Count() })
                    .ToDictionary(x => x.Gender, x => x.Count);

                // Path distribution
                var pathDistribution = students
                    .GroupBy(s => s.Path)
                    .Select(g => new { Path = g.Key.ToString(), Count = g.Count() })
                    .ToDictionary(x => x.Path, x => x.Count);

                // Academic track distribution
                var trackDistribution = students
                    .Where(s => s.Path == PathType.Academic && s.AcademicTrack.HasValue)
                    .GroupBy(s => s.AcademicTrack.Value)
                    .Select(g => new { Track = g.Key.ToString(), Count = g.Count() })
                    .ToDictionary(x => x.Track, x => x.Count);

                // Province distribution
                var provinceDistribution = students
                    .GroupBy(s => s.Province)
                    .Select(g => new ProvinceStatViewModel
                    {
                        Province = g.Key,
                        StudentCount = g.Count(),
                        AverageGPA = g.Average(s => s.GPA)
                    })
                    .OrderByDescending(p => p.StudentCount)
                    .ToList();

                // GPA ranges
                var gpaRanges = new Dictionary<string, int>
                {
                    { "90-100", students.Count(s => s.GPA >= 90) },
                    { "80-89", students.Count(s => s.GPA >= 80 && s.GPA < 90) },
                    { "70-79", students.Count(s => s.GPA >= 70 && s.GPA < 80) },
                    { "60-69", students.Count(s => s.GPA >= 60 && s.GPA < 70) },
                    { "Below 60", students.Count(s => s.GPA < 60) }
                };

                // Average GPA by path
                var gpaByPath = students
                    .GroupBy(s => s.Path)
                    .Select(g => new { Path = g.Key.ToString(), AverageGPA = g.Average(s => s.GPA) })
                    .ToDictionary(x => x.Path, x => x.AverageGPA);

                var viewModel = new DemographicsAnalyticsViewModel
                {
                    TotalStudents = students.Count,
                    AverageGPA = students.Any() ? students.Average(s => s.GPA) : 0,
                    GenderDistribution = genderDistribution,
                    PathDistribution = pathDistribution,
                    AcademicTrackDistribution = trackDistribution,
                    ProvinceDistribution = provinceDistribution,
                    GPARangeDistribution = gpaRanges,
                    AverageGPAByPath = gpaByPath
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading demographics analytics");
                TempData["Error"] = "An error occurred while loading analytics.";
                return View(new DemographicsAnalyticsViewModel());
            }
        }

        // GET: /UniversityRep/Analytics/Export
        [HttpGet("Analytics/Export")]
        public async Task<IActionResult> ExportAnalytics(string reportType = "applications")
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var rep = await _context.UniversityRepresentatives
                    .Include(r => r.University)
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

                if (rep == null)
                {
                    TempData["Error"] = "You are not assigned to any university.";
                    return RedirectToAction("Dashboard");
                }

                using var workbook = new XLWorkbook();

                switch (reportType.ToLower())
                {
                    case "applications":
                        await ExportApplicationsReport(workbook, rep.UniversityId);
                        break;
                    case "programs":
                        await ExportProgramsReport(workbook, rep.UniversityId);
                        break;
                    case "demographics":
                        await ExportDemographicsReport(workbook, rep.UniversityId);
                        break;
                    default:
                        await ExportComprehensiveReport(workbook, rep.UniversityId);
                        break;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = $"{rep.University.NameEnglish}_{reportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting analytics");
                TempData["Error"] = "An error occurred while exporting the report.";
                return RedirectToAction("Dashboard");
            }
        }

        #region Private Helper Methods

        private async Task ExportApplicationsReport(XLWorkbook workbook, int universityId)
        {
            var applications = await _context.StudentApplications
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(a => a.BtecProgram)
                .Include(a => a.UniversityProgram)
                    .ThenInclude(up => up.University)
                .Include(a => a.BtecProgram)
                    .ThenInclude(bp => bp.University)
                .Where(a => (a.UniversityProgram != null && a.UniversityProgram.UniversityId == universityId) ||
                            (a.BtecProgram != null && a.BtecProgram.UniversityId == universityId))
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            var worksheet = workbook.Worksheets.Add("Applications");

            // Headers
            worksheet.Cell(1, 1).Value = "Application Number";
            worksheet.Cell(1, 2).Value = "Student Name";
            worksheet.Cell(1, 3).Value = "Student Email";
            worksheet.Cell(1, 4).Value = "Program Name";
            worksheet.Cell(1, 5).Value = "Program Type";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Application Date";
            worksheet.Cell(1, 8).Value = "Approval Date";
            worksheet.Cell(1, 9).Value = "Processing Days";

            // Data
            for (int i = 0; i < applications.Count; i++)
            {
                var app = applications[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = app.ApplicationNumber ?? "N/A";
                worksheet.Cell(row, 2).Value = app.Student.User.FullName;
                worksheet.Cell(row, 3).Value = app.Student.User.Email;
                worksheet.Cell(row, 4).Value = app.UniversityProgram != null ?
                    app.UniversityProgram.Program.NameEnglish : app.BtecProgram.NameEnglish;
                worksheet.Cell(row, 5).Value = app.UniversityProgram != null ? "University" : "BTEC";
                worksheet.Cell(row, 6).Value = app.Status.ToString();
                worksheet.Cell(row, 7).Value = app.ApplicationDate;
                worksheet.Cell(row, 8).Value = app.ApprovalDate?.ToString() ?? "Pending";

                if (app.ApprovalDate.HasValue)
                {
                    worksheet.Cell(row, 9).Value = (app.ApprovalDate.Value - app.ApplicationDate).TotalDays;
                }
            }

            // Format
            worksheet.Range(1, 1, 1, 9).Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 9).Style.Fill.BackgroundColor = XLColor.LightBlue;
            worksheet.Columns().AdjustToContents();
        }

        private async Task ExportProgramsReport(XLWorkbook workbook, int universityId)
        {
            var worksheet = workbook.Worksheets.Add("Programs");

            worksheet.Cell(1, 1).Value = "Program Name";
            worksheet.Cell(1, 2).Value = "Type";
            worksheet.Cell(1, 3).Value = "Degree/Level";
            worksheet.Cell(1, 4).Value = "Language";
            worksheet.Cell(1, 5).Value = "Total Applications";
            worksheet.Cell(1, 6).Value = "Approved";
            worksheet.Cell(1, 7).Value = "Pending";
            worksheet.Cell(1, 8).Value = "Rejected";
            worksheet.Cell(1, 9).Value = "Capacity";
            worksheet.Cell(1, 10).Value = "Capacity Used %";

            int row = 2;

            var programs = await _context.UniversityPrograms
                .Where(p => p.UniversityId == universityId)
                .Include(p => p.Program)
                .ToListAsync();

            foreach (var program in programs)
            {
                var apps = await _context.StudentApplications
                    .Where(a => a.UniversityProgramId == program.Id)
                    .ToListAsync();

                worksheet.Cell(row, 1).Value = program.Program.NameEnglish;
                worksheet.Cell(row, 2).Value = "University";
                worksheet.Cell(row, 3).Value = program.Program.Degree.ToString();
                worksheet.Cell(row, 4).Value = program.Program.Language.ToString();
                worksheet.Cell(row, 5).Value = apps.Count;
                worksheet.Cell(row, 6).Value = apps.Count(a => a.Status == ApplicationStatus.Approved);
                worksheet.Cell(row, 7).Value = apps.Count(a => a.Status == ApplicationStatus.Pending);
                worksheet.Cell(row, 8).Value = apps.Count(a => a.Status == ApplicationStatus.Rejected);
                worksheet.Cell(row, 9).Value = program.Capacity;
                worksheet.Cell(row, 10).Value = program.Capacity > 0 ?
                    (decimal)apps.Count(a => a.Status == ApplicationStatus.Approved) / program.Capacity * 100 : 0;

                row++;
            }

            worksheet.Range(1, 1, 1, 10).Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 10).Style.Fill.BackgroundColor = XLColor.LightGreen;
            worksheet.Columns().AdjustToContents();
        }

        private async Task ExportDemographicsReport(XLWorkbook workbook, int universityId)
        {
            var students = await _context.StudentApplications
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.UniversityProgram)
                    .ThenInclude(up => up.University)
                .Include(a => a.BtecProgram)
                    .ThenInclude(bp => bp.University)
                .Where(a => (a.UniversityProgram != null && a.UniversityProgram.UniversityId == universityId) ||
                            (a.BtecProgram != null && a.BtecProgram.UniversityId == universityId))
                .Select(a => a.Student)
                .Distinct()
                .ToListAsync();

            var worksheet = workbook.Worksheets.Add("Demographics");

            worksheet.Cell(1, 1).Value = "Student Name";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "Province";
            worksheet.Cell(1, 4).Value = "City";
            worksheet.Cell(1, 5).Value = "GPA";
            worksheet.Cell(1, 6).Value = "Gender";
            worksheet.Cell(1, 7).Value = "Path";
            worksheet.Cell(1, 8).Value = "Academic Track";

            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = student.User.FullName;
                worksheet.Cell(row, 2).Value = student.User.Email;
                worksheet.Cell(row, 3).Value = student.Province;
                worksheet.Cell(row, 4).Value = student.City;
                worksheet.Cell(row, 5).Value = student.GPA;
                worksheet.Cell(row, 6).Value = student.Gender.ToString();
                worksheet.Cell(row, 7).Value = student.Path.ToString();
                worksheet.Cell(row, 8).Value = student.AcademicTrack?.ToString() ?? "N/A";
            }

            worksheet.Range(1, 1, 1, 8).Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;
            worksheet.Columns().AdjustToContents();
        }

        private async Task ExportComprehensiveReport(XLWorkbook workbook, int universityId)
        {
            await ExportApplicationsReport(workbook, universityId);
            await ExportProgramsReport(workbook, universityId);
            await ExportDemographicsReport(workbook, universityId);
        }

        #endregion
    }
}
