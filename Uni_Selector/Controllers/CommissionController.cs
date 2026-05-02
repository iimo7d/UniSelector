using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.Commission;


namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "UniversityRep")]
    [Route("UniversityRep/Commissions")]
    public class CommissionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommissionController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /UniversityRep/Commissions
        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, bool? settled, CommissionMode? mode, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                TempData["Error"] = "You do not have permission to view commissions.";
                return RedirectToAction("Index", "UniversityRep");
            }

            // Query commissions for this university
            var query = _context.Commissions
                .Include(c => c.Application)
                    .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(c => c.MonthlySettlement)
                .Where(c => c.UniversityId == rep.UniversityId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Application.ApplicationNumber.Contains(search) ||
                    c.Application.Student.User.FullName.Contains(search) ||
                    c.Application.Student.User.Email.Contains(search));
            }

            if (settled.HasValue)
            {
                query = query.Where(c => c.Settled == settled.Value);
            }

            if (mode.HasValue)
            {
                query = query.Where(c => c.Mode == mode.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.CreatedAt <= endOfDay);
            }

            // Calculate summary stats
            var allCommissions = await query.ToListAsync();
            var totalAmount = allCommissions.Sum(c => c.AmountEstimated);
            var settledAmount = allCommissions.Where(c => c.Settled).Sum(c => c.AmountEstimated);
            var pendingAmount = allCommissions.Where(c => !c.Settled).Sum(c => c.AmountEstimated);
            var settledCount = allCommissions.Count(c => c.Settled);
            var pendingCount = allCommissions.Count(c => !c.Settled);

            // Get total count
            var totalCount = allCommissions.Count;

            // Get paginated results
            var commissions = allCommissions
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommissionListItemDto
                {
                    Id = c.Id,
                    StudentName = c.Application.Student.User.FullName,
                    StudentEmail = c.Application.Student.User.Email,
                    ApplicationNumber = c.Application.ApplicationNumber,
                    AdmissionNumber = c.Application.AdmissionNumber,
                    Mode = c.Mode,
                    Percentage = c.Percentage,
                    BaseAmount = c.BaseAmount,
                    AmountEstimated = c.AmountEstimated,
                    Settled = c.Settled,
                    CreatedAt = c.CreatedAt,
                    CalculatedAt = c.CalculatedAt,
                    MonthlySettlementId = c.MonthlySettlementId,
                    SettlementPeriod = c.MonthlySettlement != null
                        ? $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(c.MonthlySettlement.Month)} {c.MonthlySettlement.Year}"
                        : null
                })
                .ToList();

            var viewModel = new CommissionListViewModel
            {
                Commissions = commissions,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Search = search,
                SettledFilter = settled,
                ModeFilter = mode,
                FromDate = fromDate,
                ToDate = toDate,
                TotalCommissionAmount = totalAmount,
                SettledAmount = settledAmount,
                PendingAmount = pendingAmount,
                SettledCount = settledCount,
                PendingCount = pendingCount,
                UniversityId = rep.UniversityId,
                UniversityName = rep.University.NameEnglish,
                CanViewCommissions = rep.CanViewApplications
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/Commissions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                TempData["Error"] = "You do not have permission to view commissions.";
                return RedirectToAction("Index");
            }

            // Load commission with all related data
            var commission = await _context.Commissions
                .Include(c => c.Application)
                    .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(c => c.Application.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(c => c.Application.BtecProgram)
                .Include(c => c.Application.DiscountGrant)
                .Include(c => c.University)
                .Include(c => c.MonthlySettlement)
                    .ThenInclude(ms => ms.ClosedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (commission == null)
            {
                TempData["Error"] = "Commission not found.";
                return RedirectToAction("Index");
            }

            // Verify commission belongs to rep's university
            if (commission.UniversityId != rep.UniversityId)
            {
                TempData["Error"] = "Commission not found.";
                return RedirectToAction("Index");
            }

            var application = commission.Application;
            var student = application.Student;
            var isBtecProgram = application.BtecProgramId.HasValue;

            var viewModel = new CommissionDetailsViewModel
            {
                // Commission Info
                CommissionId = commission.Id,
                Mode = commission.Mode,
                Percentage = commission.Percentage,
                BaseAmount = commission.BaseAmount,
                AmountEstimated = commission.AmountEstimated,
                HoursCountUsed = commission.HoursCountUsed,
                RegistrationFeeUsed = commission.RegistrationFeeUsed,
                HourPriceUsed = commission.HourPriceUsed,
                DiscountPercentApplied = commission.DiscountPercentApplied,
                Settled = commission.Settled,
                CreatedAt = commission.CreatedAt,
                CalculatedAt = commission.CalculatedAt,

                // Settlement Info
                MonthlySettlementId = commission.MonthlySettlementId,
                SettlementPeriod = commission.MonthlySettlement != null
                    ? $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(commission.MonthlySettlement.Month)} {commission.MonthlySettlement.Year}"
                    : null,
                SettlementClosedAt = commission.MonthlySettlement?.ClosedAt,
                SettlementClosedByUserName = commission.MonthlySettlement?.ClosedByUser?.FullName,

                // Application Info
                ApplicationId = application.Id,
                ApplicationNumber = application.ApplicationNumber,
                AdmissionNumber = application.AdmissionNumber,
                ApplicationStatus = application.Status,
                ApplicationDate = application.ApplicationDate,
                ApprovalDate = application.ApprovalDate,

                // Student Info
                StudentId = student.Id,
                StudentName = student.User.FullName,
                StudentEmail = student.User.Email,
                StudentPhone = student.User.PhoneNumber ?? "",
                NationalId = student.NationalId,
                SeatNumber = student.SeatNumber,
                GPA = student.GPA,
                Path = student.Path,

                // Program Info
                IsBtecProgram = isBtecProgram,
                ProgramNameEnglish = isBtecProgram ? application.BtecProgram!.NameEnglish : application.UniversityProgram!.Program.NameEnglish,
                ProgramNameArabic = isBtecProgram ? application.BtecProgram!.NameArabic : application.UniversityProgram!.Program.NameArabic,
                Degree = isBtecProgram ? null : application.UniversityProgram!.Program.Degree,
                BtecLevel = isBtecProgram ? application.BtecProgram!.Level : null,
                TotalCreditHours = isBtecProgram ? application.BtecProgram!.TotalCreditHours : application.UniversityProgram!.Program.TotalCreditHours,
                HourPriceBase = isBtecProgram ? application.BtecProgram!.HourPriceBase : application.UniversityProgram!.HourPriceBase,
                RegistrationFeeFirstSemester = isBtecProgram ? application.BtecProgram!.RegistrationFeeFirstSemester : application.UniversityProgram!.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = isBtecProgram ? application.BtecProgram!.RegistrationFeeRegularSemester : application.UniversityProgram!.RegistrationFeeRegularSemester,

                // Discount Info
                HasDiscount = application.DiscountGrant != null,
                DiscountCode = application.DiscountGrant?.Code,
                DiscountPercentage = application.DiscountGrant?.Percentage,
                DiscountAmountEstimated = application.DiscountGrant?.AmountEstimated,
                DiscountStatus = application.DiscountGrant?.Status,

                // University Info
                UniversityId = commission.University.Id,
                UniversityNameEnglish = commission.University.NameEnglish,
                UniversityNameArabic = commission.University.NameArabic
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/Commissions/Settlement/{settlementId}
        [HttpGet("Settlement/{settlementId}")]
        public async Task<IActionResult> Settlement(int settlementId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                TempData["Error"] = "You do not have permission to view settlements.";
                return RedirectToAction("Index");
            }

            // Load settlement with all related data
            var settlement = await _context.MonthlySettlements
                .Include(ms => ms.University)
                .Include(ms => ms.ClosedByUser)
                .Include(ms => ms.Commissions)
                    .ThenInclude(c => c.Application)
                    .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(ms => ms.Id == settlementId);

            if (settlement == null)
            {
                TempData["Error"] = "Settlement not found.";
                return RedirectToAction("Index");
            }

            // Verify settlement belongs to rep's university
            if (settlement.UniversityId != rep.UniversityId)
            {
                TempData["Error"] = "Settlement not found.";
                return RedirectToAction("Index");
            }

            // Build commissions list
            var commissions = settlement.Commissions.Select(c => new CommissionListItemDto
            {
                Id = c.Id,
                StudentName = c.Application.Student.User.FullName,
                StudentEmail = c.Application.Student.User.Email,
                ApplicationNumber = c.Application.ApplicationNumber,
                AdmissionNumber = c.Application.AdmissionNumber,
                Mode = c.Mode,
                Percentage = c.Percentage,
                BaseAmount = c.BaseAmount,
                AmountEstimated = c.AmountEstimated,
                Settled = c.Settled,
                CreatedAt = c.CreatedAt,
                CalculatedAt = c.CalculatedAt,
                MonthlySettlementId = c.MonthlySettlementId,
                SettlementPeriod = $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(settlement.Month)} {settlement.Year}"
            }).ToList();

            // Calculate statistics
            var avgAmount = commissions.Any() ? commissions.Average(c => c.AmountEstimated) : 0;
            var modeGroups = commissions.GroupBy(c => c.Mode).OrderByDescending(g => g.Count());
            var mostCommonMode = modeGroups.Any() ? modeGroups.First().Key : CommissionMode.FirstSemesterRegistration2Percent;

            var commissionsByMode = commissions
                .GroupBy(c => c.Mode)
                .ToDictionary(g => g.Key, g => g.Count());

            var amountByMode = commissions
                .GroupBy(c => c.Mode)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.AmountEstimated));

            var viewModel = new MonthlySettlementDetailsViewModel
            {
                // Settlement Info
                SettlementId = settlement.Id,
                Year = settlement.Year,
                Month = settlement.Month,
                PeriodDisplay = $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(settlement.Month)} {settlement.Year}",
                TotalCommission = settlement.TotalCommission,
                StudentCount = settlement.StudentCount,
                Closed = settlement.Closed,
                CreatedAt = settlement.CreatedAt,
                ClosedAt = settlement.ClosedAt,
                ClosedByUserName = settlement.ClosedByUser?.FullName,
                Notes = settlement.Notes,

                // Commissions
                Commissions = commissions,

                // Statistics
                AverageCommissionAmount = avgAmount,
                MostCommonMode = mostCommonMode,
                CommissionsByMode = commissionsByMode,
                AmountByMode = amountByMode,

                // University Info
                UniversityId = settlement.UniversityId,
                UniversityName = settlement.University.NameEnglish
            };

            return View(viewModel);
        }
    }
}