using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "BtecAuthority")]
    [Route("BTECAuthority")]
    public class BTECAuthorityController : Controller
    {
        private readonly AppDbContext _context;

        public BTECAuthorityController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /BTECAuthority/Dashboard
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new BTECDashboardViewModel();

            // ===================================
            // SUMMARY STATISTICS
            // ===================================

            viewModel.TotalBtecPrograms = await _context.BtecPrograms.CountAsync();
            viewModel.PendingApproval = await _context.BtecPrograms
                .CountAsync(bp => !bp.IsApprovedByBtecAuthority && bp.IsActive);
            viewModel.ApprovedPrograms = await _context.BtecPrograms
                .CountAsync(bp => bp.IsApprovedByBtecAuthority);
            viewModel.RejectedPrograms = await _context.BtecPrograms
                .CountAsync(bp => !bp.IsApprovedByBtecAuthority && !bp.IsActive);

            // ===================================
            // UNIVERSITY STATISTICS
            // ===================================

            viewModel.TotalUniversities = await _context.Universities
                .Where(u => u.IsActive)
                .CountAsync();
            viewModel.UniversitiesWithBtec = await _context.BtecPrograms
                .Select(bp => bp.UniversityId)
                .Distinct()
                .CountAsync();

            // ===================================
            // STUDENT STATISTICS
            // ===================================

            viewModel.TotalBtecApplications = await _context.StudentApplications
                .Where(sa => sa.BtecProgramId != null)
                .CountAsync();
            viewModel.ActiveStudents = await _context.StudentApplications
                .Where(sa => sa.BtecProgramId != null &&
                            (sa.Status == ApplicationStatus.Approved ||
                             sa.Status == ApplicationStatus.Enrolled))
                .Select(sa => sa.StudentId)
                .Distinct()
                .CountAsync();

            // ===================================
            // PROGRAMS BY LEVEL (PIE CHART)
            // ===================================

            viewModel.ProgramsByLevel = await _context.BtecPrograms
                .GroupBy(bp => bp.Level)
                .Select(g => new ProgramsByLevelDto
                {
                    Level = g.Key,
                    LevelName = $"Level {(int)g.Key}",
                    Count = g.Count(),
                    Approved = g.Count(bp => bp.IsApprovedByBtecAuthority),
                    Pending = g.Count(bp => !bp.IsApprovedByBtecAuthority && bp.IsActive)
                })
                .OrderBy(x => x.Level)
                .ToListAsync();

            // ===================================
            // PROGRAMS BY FIELD (COLUMN CHART)
            // ===================================

            var totalPrograms = viewModel.TotalBtecPrograms;
            viewModel.ProgramsByField = await _context.BtecPrograms
                .GroupBy(bp => bp.TechnicalField)
                .Select(g => new ProgramsByFieldDto
                {
                    TechnicalField = g.Key,
                    Count = g.Count(),
                    Percentage = totalPrograms > 0 ? Math.Round((g.Count() * 100.0) / totalPrograms, 2) : 0
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // ===================================
            // MONTHLY APPROVALS (LINE CHART)
            // ===================================

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var approvalsByMonth = await _context.BtecPrograms
                .Where(bp => bp.ApprovalDate >= sixMonthsAgo || bp.CreatedAt >= sixMonthsAgo)
                .ToListAsync();

            viewModel.MonthlyApprovals = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var targetMonth = DateTime.UtcNow.AddMonths(-5 + i);
                    var monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    return new MonthlyApprovalDto
                    {
                        Month = targetMonth.Month,
                        MonthName = targetMonth.ToString("MMM"),
                        Approved = approvalsByMonth.Count(bp =>
                            bp.ApprovalDate >= monthStart &&
                            bp.ApprovalDate < monthEnd &&
                            bp.IsApprovedByBtecAuthority),
                        Rejected = approvalsByMonth.Count(bp =>
                            bp.CreatedAt >= monthStart &&
                            bp.CreatedAt < monthEnd &&
                            !bp.IsApprovedByBtecAuthority &&
                            !bp.IsActive),
                        Pending = approvalsByMonth.Count(bp =>
                            bp.CreatedAt >= monthStart &&
                            bp.CreatedAt < monthEnd &&
                            !bp.IsApprovedByBtecAuthority &&
                            bp.IsActive)
                    };
                })
                .ToList();

            // ===================================
            // TOP UNIVERSITIES (TABLE)
            // ===================================

            viewModel.TopUniversities = await _context.BtecPrograms
                .Include(bp => bp.University)
                .GroupBy(bp => new { bp.UniversityId, bp.University.NameEnglish })
                .Select(g => new UniversityProgramCountDto
                {
                    UniversityId = g.Key.UniversityId,
                    UniversityName = g.Key.NameEnglish,
                    ProgramCount = g.Count(),
                    ApprovedCount = g.Count(bp => bp.IsApprovedByBtecAuthority),
                    PendingCount = g.Count(bp => !bp.IsApprovedByBtecAuthority && bp.IsActive)
                })
                .OrderByDescending(x => x.ProgramCount)
                .Take(10)
                .ToListAsync();

            // ===================================
            // RECENT PROGRAMS
            // ===================================

            viewModel.RecentPrograms = await _context.BtecPrograms
                .Include(bp => bp.University)
                .OrderByDescending(bp => bp.CreatedAt)
                .Take(10)
                .Select(bp => new RecentBtecProgramDto
                {
                    Id = bp.Id,
                    NameEnglish = bp.NameEnglish,
                    UniversityName = bp.University.NameEnglish,
                    Level = bp.Level,
                    TechnicalField = bp.TechnicalField,
                    IsApprovedByBtecAuthority = bp.IsApprovedByBtecAuthority,
                    CreatedAt = bp.CreatedAt,
                    ApprovalDate = bp.ApprovalDate
                })
                .ToListAsync();

            return View(viewModel);
        }
    }
}