using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "BtecAuthority")]
    [Route("BTECAuthority/Reports")]
    public class BTECReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BTECReportsController> _logger;

        public BTECReportsController(AppDbContext context, ILogger<BTECReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Reports Dashboard

        /// <summary>
        /// Main reports dashboard - GET /BTECAuthority/Reports
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new BTECReportsDashboardViewModel();

                // Summary Statistics
                model.TotalBtecPrograms = await _context.BtecPrograms.CountAsync();
                model.ActiveBtecPrograms = await _context.BtecPrograms.CountAsync(p => p.IsActive && p.IsApprovedByBtecAuthority);

                model.TotalBtecStudents = await _context.Students
                    .CountAsync(s => s.Path == PathType.BTEC || s.BtecLevel2Completed || s.BtecLevel3Completed);

                model.TotalBtecApplications = await _context.StudentApplications
                    .CountAsync(a => a.BtecProgramId != null);

                model.UniversitiesOfferingBtec = await _context.BtecPrograms
                    .Select(p => p.UniversityId)
                    .Distinct()
                    .CountAsync();

                // Monthly Trends (Last 6 months)
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var applications = await _context.StudentApplications
                    .Where(a => a.BtecProgramId != null && a.ApplicationDate >= sixMonthsAgo)
                    .ToListAsync();

                model.MonthlyTrends = applications
                    .GroupBy(a => new { a.ApplicationDate.Year, a.ApplicationDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new MonthlyTrendDto
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Applications = g.Count(),
                        Approvals = g.Count(a => a.Status == ApplicationStatus.Approved),
                        Enrollments = g.Count(a => a.Status == ApplicationStatus.Enrolled)
                    })
                    .ToList();

                // Compliance by Level
                var programsByLevel = await _context.BtecPrograms
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.Level)
                    .Select(g => new
                    {
                        Level = g.Key,
                        Count = g.Count(),
                        Approved = g.Count(p => p.IsApprovedByBtecAuthority)
                    })
                    .ToListAsync();

                model.ComplianceByLevel = programsByLevel.Select(p => new ProgramComplianceDto
                {
                    Level = p.Level,
                    TotalStudents = p.Count,
                    ActiveStudents = p.Approved
                }).ToList();

                // Top Universities
                var universityStats = await _context.BtecPrograms
                    .Include(p => p.University)
                    .GroupBy(p => new { p.UniversityId, p.University.NameEnglish, p.University.City })
                    .Select(g => new UniversityBtecStatsDto
                    {
                        UniversityId = g.Key.UniversityId,
                        UniversityName = g.Key.NameEnglish,
                        City = g.Key.City,
                        TotalPrograms = g.Count(),
                        ApprovedPrograms = g.Count(p => p.IsApprovedByBtecAuthority),
                        TotalStudents = 0, // Will calculate separately
                        ComplianceRate = g.Count(p => p.IsApprovedByBtecAuthority) * 100.0 / g.Count()
                    })
                    .OrderByDescending(u => u.TotalPrograms)
                    .Take(10)
                    .ToListAsync();

                model.TopUniversities = universityStats;

                // Programs by Field
                var fieldStats = await _context.BtecPrograms
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.TechnicalField)
                    .Select(g => new FieldStatisticsDto
                    {
                        TechnicalField = g.Key,
                        ProgramCount = g.Count(),
                        StudentCount = 0, // Will calculate if needed
                        UniversityCount = g.Select(p => p.UniversityId).Distinct().Count(),
                        AverageCapacity = g.Average(p => p.Capacity)
                    })
                    .OrderByDescending(f => f.ProgramCount)
                    .Take(10)
                    .ToListAsync();

                model.ProgramsByField = fieldStats;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading BTEC reports dashboard");
                TempData["Error"] = "Failed to load reports dashboard";
                return RedirectToAction("Dashboard", "BTECAuthority");
            }
        }

        #endregion

        #region Program Compliance Report

        /// <summary>
        /// Program compliance report - GET /BTECAuthority/Reports/Programs
        /// </summary>
        [HttpGet("Programs")]
        public async Task<IActionResult> Programs(
            string? searchTerm,
            BtecLevel? level,
            string? technicalField,
            int? universityId,
            ComplianceStatus? complianceStatus,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var model = new ProgramComplianceReportViewModel
                {
                    SearchTerm = searchTerm,
                    Level = level,
                    TechnicalField = technicalField,
                    UniversityId = universityId,
                    ComplianceStatus = complianceStatus,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Build query
                var query = _context.BtecPrograms
                    .Include(p => p.University)
                    .Where(p => p.IsActive)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.NameEnglish.Contains(searchTerm) ||
                        p.NameArabic.Contains(searchTerm) ||
                        p.University.NameEnglish.Contains(searchTerm) ||
                        p.TechnicalField.Contains(searchTerm));
                }

                if (level.HasValue)
                    query = query.Where(p => p.Level == level.Value);

                if (!string.IsNullOrWhiteSpace(technicalField))
                    query = query.Where(p => p.TechnicalField == technicalField);

                if (universityId.HasValue)
                    query = query.Where(p => p.UniversityId == universityId.Value);

                // Apply compliance filter (simplified logic)
                if (complianceStatus.HasValue)
                {
                    switch (complianceStatus.Value)
                    {
                        case ComplianceStatus.FullyCompliant:
                            query = query.Where(p => p.IsApprovedByBtecAuthority && p.Capacity > 0);
                            break;
                        case ComplianceStatus.NonCompliant:
                            query = query.Where(p => !p.IsApprovedByBtecAuthority);
                            break;
                    }
                }

                // Get total count
                model.TotalRecords = await query.CountAsync();
                model.TotalPages = (int)Math.Ceiling(model.TotalRecords / (double)pageSize);

                // Get paginated data
                var programs = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                model.Programs = programs.Select(p => new ProgramComplianceDto
                {
                    ProgramId = p.Id,
                    ProgramName = p.NameEnglish,
                    University = p.University.NameEnglish,
                    UniversityCity = p.University.City,
                    Level = p.Level,
                    TechnicalField = p.TechnicalField,
                    ComplianceStatus = DetermineComplianceStatus(p),
                    TotalStudents = _context.StudentApplications.Count(a => a.BtecProgramId == p.Id),
                    ActiveStudents = _context.StudentApplications.Count(a => a.BtecProgramId == p.Id &&
                        (a.Status == ApplicationStatus.Enrolled || a.Status == ApplicationStatus.Approved)),
                    LastReviewDate = p.ApprovalDate,
                    ComplianceNotes = p.ApprovalNotes
                }).ToList();

                // Get filter options
                model.TechnicalFields = await _context.BtecPrograms
                    .Where(p => p.IsActive)
                    .Select(p => p.TechnicalField)
                    .Distinct()
                    .OrderBy(f => f)
                    .ToListAsync();

                model.Universities = await _context.Universities
                    .Where(u => u.BtecPrograms.Any(p => p.IsActive))
                    .Select(u => new UniversityOptionDto
                    {
                        Id = u.Id,
                        NameEnglish = u.NameEnglish,
                        ProgramCount = u.BtecPrograms.Count(p => p.IsActive)
                    })
                    .OrderBy(u => u.NameEnglish)
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program compliance report");
                TempData["Error"] = "Failed to load program compliance report";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Student Statistics Report

        /// <summary>
        /// BTEC student statistics - GET /BTECAuthority/Reports/Students
        /// </summary>
        [HttpGet("Students")]
        public async Task<IActionResult> Students()
        {
            try
            {
                var model = new BTECStudentStatisticsViewModel();

                // Summary statistics
                var btecStudents = await _context.Students
                    .Where(s => s.Path == PathType.BTEC || s.BtecLevel2Completed || s.BtecLevel3Completed)
                    .ToListAsync();

                model.TotalStudents = btecStudents.Count;
                model.ActiveStudents = btecStudents.Count(s => s.IsActive);
                model.StudentsWithLevel2 = btecStudents.Count(s => s.BtecLevel2Completed);
                model.StudentsWithLevel3 = btecStudents.Count(s => s.BtecLevel3Completed);

                model.EnrolledStudents = await _context.StudentApplications
                    .CountAsync(a => a.BtecProgramId != null && a.Status == ApplicationStatus.Enrolled);

                // Gender distribution
                var genderGroups = btecStudents.GroupBy(s => s.Gender).ToList();
                model.GenderDistribution = genderGroups.Select(g => new GenderDistributionDto
                {
                    Gender = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = model.TotalStudents > 0 ? (g.Count() * 100.0) / model.TotalStudents : 0
                }).ToList();

                // Province distribution
                var provinceGroups = btecStudents.GroupBy(s => s.Province).ToList();
                model.ProvinceDistribution = provinceGroups.Select(g => new ProvinceDistributionDto
                {
                    Province = g.Key,
                    Count = g.Count(),
                    Percentage = model.TotalStudents > 0 ? (g.Count() * 100.0) / model.TotalStudents : 0
                }).OrderByDescending(p => p.Count).ToList();

                // Enrollment by level
                var enrollmentByLevel = await _context.StudentApplications
                    .Include(a => a.BtecProgram)
                    .Where(a => a.BtecProgramId != null)
                    .GroupBy(a => a.BtecProgram.Level)
                    .Select(g => new LevelEnrollmentDto
                    {
                        Level = g.Key,
                        EnrolledCount = g.Count(a => a.Status == ApplicationStatus.Enrolled),
                        CompletedCount = 0, // Would need completion tracking
                        CompletionRate = 0
                    })
                    .ToListAsync();

                model.EnrollmentByLevel = enrollmentByLevel;

                // GPA statistics
                model.AverageGPA = btecStudents.Any() ? btecStudents.Average(s => s.GPA) : 0;

                var gpaRanges = new[]
                {
                    new { Range = "90-100", Min = 90.0, Max = 100.0 },
                    new { Range = "80-89", Min = 80.0, Max = 89.9 },
                    new { Range = "70-79", Min = 70.0, Max = 79.9 },
                    new { Range = "60-69", Min = 60.0, Max = 69.9 },
                    new { Range = "Below 60", Min = 0.0, Max = 59.9 }
                };

                model.GPADistribution = gpaRanges.Select(r => new GPARangeDto
                {
                    Range = r.Range,
                    Count = btecStudents.Count(s => s.GPA >= r.Min && s.GPA <= r.Max),
                    Percentage = btecStudents.Any() ? (btecStudents.Count(s => s.GPA >= r.Min && s.GPA <= r.Max) * 100.0) / model.TotalStudents : 0
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student statistics");
                TempData["Error"] = "Failed to load student statistics";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Universities BTEC Report

        /// <summary>
        /// Universities offering BTEC - GET /BTECAuthority/Reports/Universities
        /// </summary>
        [HttpGet("Universities")]
        public async Task<IActionResult> Universities(
            string? searchTerm,
            string? province,
            string? city,
            bool? hasApprovedPrograms,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var model = new UniversitiesBTECReportViewModel
                {
                    SearchTerm = searchTerm,
                    Province = province,
                    City = city,
                    HasApprovedPrograms = hasApprovedPrograms,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Build query
                var query = _context.Universities
                    .Where(u => u.BtecPrograms.Any())
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(u =>
                        u.NameEnglish.Contains(searchTerm) ||
                        u.NameArabic.Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(province))
                    query = query.Where(u => u.Province == province);

                if (!string.IsNullOrWhiteSpace(city))
                    query = query.Where(u => u.City == city);

                if (hasApprovedPrograms.HasValue && hasApprovedPrograms.Value)
                    query = query.Where(u => u.BtecPrograms.Any(p => p.IsApprovedByBtecAuthority));

                // Get total count
                model.TotalRecords = await query.CountAsync();
                model.TotalPages = (int)Math.Ceiling(model.TotalRecords / (double)pageSize);

                // Get paginated data
                var universities = await query
                    .Include(u => u.BtecPrograms)
                    .OrderBy(u => u.NameEnglish)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                model.Universities = universities.Select(u => new UniversityBtecDetailDto
                {
                    UniversityId = u.Id,
                    NameEnglish = u.NameEnglish,
                    NameArabic = u.NameArabic,
                    Province = u.Province,
                    City = u.City,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    TotalBtecPrograms = u.BtecPrograms.Count,
                    ApprovedPrograms = u.BtecPrograms.Count(p => p.IsApprovedByBtecAuthority),
                    PendingPrograms = u.BtecPrograms.Count(p => !p.IsApprovedByBtecAuthority),
                    TotalStudents = _context.StudentApplications.Count(a =>
                        a.BtecProgramId != null &&
                        u.BtecPrograms.Select(p => p.Id).Contains(a.BtecProgramId.Value)),
                    FirstProgramDate = u.BtecPrograms.Any() ? u.BtecPrograms.Min(p => p.CreatedAt) : (DateTime?)null,
                    LevelsOffered = u.BtecPrograms.Select(p => p.Level).Distinct().ToList()
                }).ToList();

                // Get filter options
                model.Provinces = await _context.Universities
                    .Where(u => u.BtecPrograms.Any())
                    .Select(u => u.Province)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync();

                model.Cities = await _context.Universities
                    .Where(u => u.BtecPrograms.Any())
                    .Select(u => u.City)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading universities BTEC report");
                TempData["Error"] = "Failed to load universities report";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Export Report

        /// <summary>
        /// Export reports - GET /BTECAuthority/Reports/Export
        /// </summary>
        [HttpGet("Export")]
        public async Task<IActionResult> Export(
            ReportType reportType,
            ExportType exportType = ExportType.Excel,
            BtecLevel? level = null,
            string? technicalField = null)
        {
            try
            {
                byte[] fileBytes;
                string fileName;
                string contentType;

                switch (exportType)
                {
                    case ExportType.Excel:
                        fileBytes = await GenerateExcelReport(reportType, level, technicalField);
                        fileName = $"BTEC_{reportType}_Report_{DateTime.Now:yyyyMMdd}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case ExportType.CSV:
                        fileBytes = await GenerateCSVReport(reportType, level, technicalField);
                        fileName = $"BTEC_{reportType}_Report_{DateTime.Now:yyyyMMdd}.csv";
                        contentType = "text/csv";
                        break;

                    default:
                        TempData["Error"] = "Export type not supported";
                        return RedirectToAction("Index");
                }

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                TempData["Error"] = "Failed to export report";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Private Helper Methods

        private ComplianceStatus DetermineComplianceStatus(BtecProgram program)
        {
            if (!program.IsApprovedByBtecAuthority)
                return ComplianceStatus.NonCompliant;

            if (program.Capacity == 0 || program.TotalCreditHours < 60)
                return ComplianceStatus.MajorIssues;

            if (string.IsNullOrWhiteSpace(program.Description))
                return ComplianceStatus.MinorIssues;

            return ComplianceStatus.FullyCompliant;
        }

        private async Task<byte[]> GenerateExcelReport(ReportType reportType, BtecLevel? level, string? technicalField)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BTEC Report");

            switch (reportType)
            {
                case ReportType.ProgramCompliance:
                    await GenerateProgramComplianceExcel(worksheet, level, technicalField);
                    break;

                case ReportType.StudentStatistics:
                    await GenerateStudentStatisticsExcel(worksheet);
                    break;

                case ReportType.UniversityPerformance:
                    await GenerateUniversityPerformanceExcel(worksheet);
                    break;

                default:
                    await GenerateOverviewExcel(worksheet);
                    break;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private async Task GenerateProgramComplianceExcel(IXLWorksheet worksheet, BtecLevel? level, string? technicalField)
        {
            // Headers
            worksheet.Cell(1, 1).Value = "Program Name";
            worksheet.Cell(1, 2).Value = "University";
            worksheet.Cell(1, 3).Value = "City";
            worksheet.Cell(1, 4).Value = "Level";
            worksheet.Cell(1, 5).Value = "Technical Field";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Total Students";
            worksheet.Cell(1, 8).Value = "Active Students";
            worksheet.Cell(1, 9).Value = "Compliance Status";

            // Data
            var query = _context.BtecPrograms
                .Include(p => p.University)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (level.HasValue)
                query = query.Where(p => p.Level == level.Value);

            if (!string.IsNullOrWhiteSpace(technicalField))
                query = query.Where(p => p.TechnicalField == technicalField);

            var programs = await query.ToListAsync();

            int row = 2;
            foreach (var program in programs)
            {
                worksheet.Cell(row, 1).Value = program.NameEnglish;
                worksheet.Cell(row, 2).Value = program.University.NameEnglish;
                worksheet.Cell(row, 3).Value = program.University.City;
                worksheet.Cell(row, 4).Value = program.Level.ToString();
                worksheet.Cell(row, 5).Value = program.TechnicalField;
                worksheet.Cell(row, 6).Value = program.IsApprovedByBtecAuthority ? "Approved" : "Pending";
                worksheet.Cell(row, 7).Value = _context.StudentApplications.Count(a => a.BtecProgramId == program.Id);
                worksheet.Cell(row, 8).Value = _context.StudentApplications.Count(a => a.BtecProgramId == program.Id && a.Status == ApplicationStatus.Enrolled);
                worksheet.Cell(row, 9).Value = DetermineComplianceStatus(program).ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        private async Task GenerateStudentStatisticsExcel(IXLWorksheet worksheet)
        {
            // Headers
            worksheet.Cell(1, 1).Value = "Metric";
            worksheet.Cell(1, 2).Value = "Value";

            var btecStudents = await _context.Students
                .Where(s => s.Path == PathType.BTEC || s.BtecLevel2Completed || s.BtecLevel3Completed)
                .ToListAsync();

            int row = 2;
            worksheet.Cell(row++, 1).Value = "Total BTEC Students";
            worksheet.Cell(row - 1, 2).Value = btecStudents.Count;

            worksheet.Cell(row++, 1).Value = "Active Students";
            worksheet.Cell(row - 1, 2).Value = btecStudents.Count(s => s.IsActive);

            worksheet.Cell(row++, 1).Value = "Level 2 Completed";
            worksheet.Cell(row - 1, 2).Value = btecStudents.Count(s => s.BtecLevel2Completed);

            worksheet.Cell(row++, 1).Value = "Level 3 Completed";
            worksheet.Cell(row - 1, 2).Value = btecStudents.Count(s => s.BtecLevel3Completed);

            worksheet.Cell(row++, 1).Value = "Average GPA";
            worksheet.Cell(row - 1, 2).Value = btecStudents.Any() ? btecStudents.Average(s => s.GPA) : 0;

            worksheet.Columns().AdjustToContents();
        }

        private async Task GenerateUniversityPerformanceExcel(IXLWorksheet worksheet)
        {
            // Headers
            worksheet.Cell(1, 1).Value = "University";
            worksheet.Cell(1, 2).Value = "City";
            worksheet.Cell(1, 3).Value = "Total Programs";
            worksheet.Cell(1, 4).Value = "Approved Programs";
            worksheet.Cell(1, 5).Value = "Total Students";
            worksheet.Cell(1, 6).Value = "Compliance Rate";

            var universities = await _context.Universities
                .Include(u => u.BtecPrograms)
                .Where(u => u.BtecPrograms.Any())
                .ToListAsync();

            int row = 2;
            foreach (var university in universities)
            {
                worksheet.Cell(row, 1).Value = university.NameEnglish;
                worksheet.Cell(row, 2).Value = university.City;
                worksheet.Cell(row, 3).Value = university.BtecPrograms.Count;
                worksheet.Cell(row, 4).Value = university.BtecPrograms.Count(p => p.IsApprovedByBtecAuthority);
                worksheet.Cell(row, 5).Value = _context.StudentApplications.Count(a =>
                    a.BtecProgramId != null &&
                    university.BtecPrograms.Select(p => p.Id).Contains(a.BtecProgramId.Value));
                worksheet.Cell(row, 6).Value = university.BtecPrograms.Any() ? $"{(university.BtecPrograms.Count(p => p.IsApprovedByBtecAuthority) * 100.0 / university.BtecPrograms.Count):F2}%" : "0%";
                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        private async Task GenerateOverviewExcel(IXLWorksheet worksheet)
        {
            // Summary statistics
            worksheet.Cell(1, 1).Value = "BTEC Programs Overview";
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            int row = 3;
            worksheet.Cell(row++, 1).Value = "Total Programs";
            worksheet.Cell(row - 1, 2).Value = await _context.BtecPrograms.CountAsync();

            worksheet.Cell(row++, 1).Value = "Approved Programs";
            worksheet.Cell(row - 1, 2).Value = await _context.BtecPrograms.CountAsync(p => p.IsApprovedByBtecAuthority);

            worksheet.Cell(row++, 1).Value = "Total Students";
            worksheet.Cell(row - 1, 2).Value = await _context.Students.CountAsync(s => s.Path == PathType.BTEC);

            worksheet.Cell(row++, 1).Value = "Universities Offering BTEC";
            worksheet.Cell(row - 1, 2).Value = await _context.BtecPrograms.Select(p => p.UniversityId).Distinct().CountAsync();

            worksheet.Columns().AdjustToContents();
        }

        private async Task<byte[]> GenerateCSVReport(ReportType reportType, BtecLevel? level, string? technicalField)
        {
            var csv = new System.Text.StringBuilder();

            switch (reportType)
            {
                case ReportType.ProgramCompliance:
                    csv.AppendLine("Program Name,University,City,Level,Technical Field,Status,Total Students,Active Students");
                    var programs = await _context.BtecPrograms
                        .Include(p => p.University)
                        .Where(p => p.IsActive)
                        .ToListAsync();

                    foreach (var program in programs)
                    {
                        csv.AppendLine($"\"{program.NameEnglish}\",\"{program.University.NameEnglish}\",\"{program.University.City}\",\"{program.Level}\",\"{program.TechnicalField}\",\"{(program.IsApprovedByBtecAuthority ? "Approved" : "Pending")}\",{_context.StudentApplications.Count(a => a.BtecProgramId == program.Id)},{_context.StudentApplications.Count(a => a.BtecProgramId == program.Id && a.Status == ApplicationStatus.Enrolled)}");
                    }
                    break;

                case ReportType.StudentStatistics:
                    csv.AppendLine("Student Name,Email,Gender,Province,Path,BTEC Level 2,BTEC Level 3,Active");
                    var students = await _context.Students
                        .Include(s => s.User)
                        .Where(s => s.Path == PathType.BTEC || s.BtecLevel2Completed || s.BtecLevel3Completed)
                        .ToListAsync();
                    foreach (var s in students)
                    {
                        csv.AppendLine($"\"{s.User?.FullName}\",\"{s.User?.Email}\",\"{s.Gender}\",\"{s.Province}\",\"{s.Path}\",\"{s.BtecLevel2Completed}\",\"{s.BtecLevel3Completed}\",\"{s.IsActive}\"");
                    }
                    break;

                case ReportType.UniversityPerformance:
                    csv.AppendLine("University,City,Total Programs,Active Programs,Approved Programs,Total Enrollments");
                    var universities = await _context.Universities
                        .Include(u => u.BtecPrograms)
                        .ToListAsync();
                    foreach (var uni in universities)
                    {
                        var totalPrograms = uni.BtecPrograms.Count;
                        var activePrograms = uni.BtecPrograms.Count(p => p.IsActive);
                        var approvedPrograms = uni.BtecPrograms.Count(p => p.IsApprovedByBtecAuthority);
                        var enrollments = await _context.StudentApplications
                            .CountAsync(a => a.BtecProgram != null && a.BtecProgram.UniversityId == uni.Id && a.Status == ApplicationStatus.Enrolled);
                        csv.AppendLine($"\"{uni.NameEnglish}\",\"{uni.City}\",{totalPrograms},{activePrograms},{approvedPrograms},{enrollments}");
                    }
                    break;

                default:
                    csv.AppendLine("Report Type,Status");
                    csv.AppendLine($"\"{reportType}\",\"No data available for this report type\"");
                    break;
            }

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        #endregion
    }
}