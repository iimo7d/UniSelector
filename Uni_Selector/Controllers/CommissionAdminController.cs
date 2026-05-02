using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.AdminCommission;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.PlatformAdmin)]
    public class CommissionAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CommissionAdminController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommissionAdminController(
            AppDbContext context,
            ILogger<CommissionAdminController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        #region Commissions

        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            int? universityId = null,
            bool? settled = null,
            CommissionMode? mode = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _context.Commissions
                    .Include(c => c.Application)
                        .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(c => c.Application.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(c => c.Application.BtecProgram)
                    .Include(c => c.University)
                    .Include(c => c.MonthlySettlement)
                    .AsQueryable();

                // Apply filters
                if (universityId.HasValue)
                {
                    query = query.Where(c => c.UniversityId == universityId.Value);
                }

                if (settled.HasValue)
                {
                    query = query.Where(c => c.Settled == settled.Value);
                }

                if (mode.HasValue)
                {
                    query = query.Where(c => c.Mode == mode.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= endDate.Value.AddDays(1));
                }

                // Calculate summary
                var allCommissions = await query.ToListAsync();
                var totalAmount = allCommissions.Sum(c => c.AmountEstimated);
                var settledAmount = allCommissions.Where(c => c.Settled).Sum(c => c.AmountEstimated);
                var pendingAmount = allCommissions.Where(c => !c.Settled).Sum(c => c.AmountEstimated);

                var totalCommissions = allCommissions.Count;

                var commissions = allCommissions
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CommissionListItemViewModel
                    {
                        Id = c.Id,
                        ApplicationNumber = c.Application.ApplicationNumber ?? $"APP-{c.ApplicationId:D6}",
                        StudentName = c.Application.Student.User.FullName,
                        UniversityName = c.University.NameEnglish,
                        Mode = c.Mode,
                        ModeText = c.Mode.ToString(),
                        Percentage = c.Percentage,
                        BaseAmount = c.BaseAmount,
                        AmountEstimated = c.AmountEstimated,
                        Settled = c.Settled,
                        CreatedAt = c.CreatedAt,
                        CalculatedAt = c.CalculatedAt,
                        MonthlySettlementId = c.MonthlySettlementId,
                        SettlementPeriod = c.MonthlySettlement != null
                            ? new DateTime(c.MonthlySettlement.Year, c.MonthlySettlement.Month, 1).ToString("MMM yyyy")
                            : null
                    })
                    .ToList();

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

                var viewModel = new CommissionListViewModel
                {
                    Commissions = commissions,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCommissions = totalCommissions,
                    UniversityId = universityId,
                    Settled = settled,
                    Mode = mode,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalCommissionAmount = totalAmount,
                    SettledAmount = settledAmount,
                    PendingAmount = pendingAmount,
                    SettledCount = allCommissions.Count(c => c.Settled),
                    PendingCount = allCommissions.Count(c => !c.Settled),
                    Universities = universities,
                    AllModes = Enum.GetValues(typeof(CommissionMode)).Cast<CommissionMode>().ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading commissions list");
                TempData["Error"] = "An error occurred while loading commissions.";
                return View(new CommissionListViewModel());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var commission = await _context.Commissions
                    .Include(c => c.Application)
                        .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(c => c.Application.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(c => c.Application.BtecProgram)
                    .Include(c => c.University)
                    .Include(c => c.MonthlySettlement)
                        .ThenInclude(ms => ms.ClosedByUser)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (commission == null)
                {
                    TempData["Error"] = "Commission not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CommissionDetailsViewModel
                {
                    Id = commission.Id,
                    Mode = commission.Mode,
                    ModeText = commission.Mode.ToString(),
                    Percentage = commission.Percentage,
                    BaseAmount = commission.BaseAmount,
                    AmountEstimated = commission.AmountEstimated,
                    Settled = commission.Settled,
                    CreatedAt = commission.CreatedAt,
                    CalculatedAt = commission.CalculatedAt,
                    HoursCountUsed = commission.HoursCountUsed,
                    RegistrationFeeUsed = commission.RegistrationFeeUsed,
                    HourPriceUsed = commission.HourPriceUsed,
                    DiscountPercentApplied = commission.DiscountPercentApplied,
                    ApplicationId = commission.ApplicationId,
                    ApplicationNumber = commission.Application.ApplicationNumber ?? $"APP-{commission.ApplicationId:D6}",
                    ApplicationStatus = commission.Application.Status,
                    ApplicationDate = commission.Application.ApplicationDate,
                    StudentName = commission.Application.Student.User.FullName,
                    StudentEmail = commission.Application.Student.User.Email,
                    UniversityId = commission.UniversityId,
                    UniversityName = commission.University.NameEnglish,
                    UniversityEmail = commission.University.Email,
                    ProgramName = commission.Application.UniversityProgram != null
                        ? commission.Application.UniversityProgram.Program.NameEnglish
                        : commission.Application.BtecProgram.NameEnglish,
                    Degree = commission.Application.UniversityProgram != null
                        ? commission.Application.UniversityProgram.Program.Degree.ToString()
                        : "BTEC " + commission.Application.BtecProgram.Level.ToString(),
                    MonthlySettlementId = commission.MonthlySettlementId,
                    SettlementPeriod = commission.MonthlySettlement != null
                        ? new DateTime(commission.MonthlySettlement.Year, commission.MonthlySettlement.Month, 1).ToString("MMMM yyyy")
                        : null,
                    SettlementClosed = commission.MonthlySettlement?.Closed,
                    SettlementClosedAt = commission.MonthlySettlement?.ClosedAt
                };

                viewModel.ModeDescription = viewModel.GetModeDescription();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading commission details for ID: {id}");
                TempData["Error"] = "An error occurred while loading commission details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate()
        {
            try
            {
                // Get all approved applications that don't have commissions yet
                var applicationsNeedingCommission = await _context.StudentApplications
                    .Include(a => a.Commission)
                    .Include(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                    .Include(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                    .Where(a => a.Status == ApplicationStatus.Approved && a.Commission == null)
                    .ToListAsync();

                int calculatedCount = 0;

                foreach (var application in applicationsNeedingCommission)
                {
                    var university = application.UniversityProgram?.University ?? application.BtecProgram?.University;
                    if (university == null) continue;

                    decimal baseAmount = 0;
                    int? hoursUsed = null;
                    decimal? regFeeUsed = null;
                    decimal? hourPriceUsed = null;

                    switch (university.CommissionMode)
                    {
                        case CommissionMode.FirstSemesterRegistration2Percent:
                            baseAmount = application.UniversityProgram?.RegistrationFeeFirstSemester
                                ?? application.BtecProgram.RegistrationFeeFirstSemester;
                            regFeeUsed = baseAmount;
                            break;

                        case CommissionMode.ProgramTotalHours2Percent:
                            var totalHours = application.UniversityProgram?.Program.TotalCreditHours
                                ?? application.BtecProgram.TotalCreditHours;
                            var hourPrice = application.UniversityProgram?.HourPriceBase
                                ?? application.BtecProgram.HourPriceBase;

                            if (application.HourDiscountPercent.HasValue)
                            {
                                hourPrice = hourPrice * (1 - application.HourDiscountPercent.Value / 100);
                            }

                            baseAmount = totalHours * hourPrice;
                            hoursUsed = totalHours;
                            hourPriceUsed = hourPrice;
                            break;

                        case CommissionMode.FirstSemesterRegPlusHours2Percent:
                            regFeeUsed = application.UniversityProgram?.RegistrationFeeFirstSemester
                                ?? application.BtecProgram.RegistrationFeeFirstSemester;

                            if (application.PlannedFirstSemesterHours.HasValue)
                            {
                                var plannedHourPrice = application.UniversityProgram?.HourPriceBase
                                    ?? application.BtecProgram.HourPriceBase;

                                if (application.HourDiscountPercent.HasValue)
                                {
                                    plannedHourPrice = plannedHourPrice * (1 - application.HourDiscountPercent.Value / 100);
                                }

                                baseAmount = regFeeUsed.Value + (application.PlannedFirstSemesterHours.Value * plannedHourPrice);
                                hoursUsed = application.PlannedFirstSemesterHours.Value;
                                hourPriceUsed = plannedHourPrice;
                            }
                            else
                            {
                                baseAmount = regFeeUsed.Value;
                            }
                            break;
                    }

                    var commission = new Commission
                    {
                        ApplicationId = application.Id,
                        UniversityId = university.Id,
                        Mode = university.CommissionMode,
                        Percentage = 2m,
                        BaseAmount = baseAmount,
                        AmountEstimated = baseAmount * 0.02m,
                        HoursCountUsed = hoursUsed,
                        RegistrationFeeUsed = regFeeUsed,
                        HourPriceUsed = hourPriceUsed,
                        DiscountPercentApplied = application.HourDiscountPercent,
                        CalculatedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        Settled = false
                    };

                    _context.Commissions.Add(commission);
                    calculatedCount++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Calculated {calculatedCount} new commissions");
                TempData["Success"] = $"Successfully calculated {calculatedCount} commission(s).";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating commissions");
                TempData["Error"] = "An error occurred while calculating commissions.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Settlements

        public async Task<IActionResult> Settlements(
            int page = 1,
            int pageSize = 10,
            int? universityId = null,
            bool? closed = null,
            int? year = null,
            int? month = null)
        {
            try
            {
                var query = _context.MonthlySettlements
                    .Include(ms => ms.University)
                    .Include(ms => ms.ClosedByUser)
                    .AsQueryable();

                // Apply filters
                if (universityId.HasValue)
                {
                    query = query.Where(ms => ms.UniversityId == universityId.Value);
                }

                if (closed.HasValue)
                {
                    query = query.Where(ms => ms.Closed == closed.Value);
                }

                if (year.HasValue)
                {
                    query = query.Where(ms => ms.Year == year.Value);
                }

                if (month.HasValue)
                {
                    query = query.Where(ms => ms.Month == month.Value);
                }

                var allSettlements = await query.ToListAsync();
                var totalAmount = allSettlements.Sum(ms => ms.TotalCommission);
                var closedAmount = allSettlements.Where(ms => ms.Closed).Sum(ms => ms.TotalCommission);
                var openAmount = allSettlements.Where(ms => !ms.Closed).Sum(ms => ms.TotalCommission);

                var totalSettlements = allSettlements.Count;

                var settlements = allSettlements
                    .OrderByDescending(ms => ms.Year)
                    .ThenByDescending(ms => ms.Month)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ms => new SettlementListItemViewModel
                    {
                        Id = ms.Id,
                        UniversityName = ms.University.NameEnglish,
                        Year = ms.Year,
                        Month = ms.Month,
                        Period = new DateTime(ms.Year, ms.Month, 1).ToString("MMMM yyyy"),
                        TotalCommission = ms.TotalCommission,
                        StudentCount = ms.StudentCount,
                        Closed = ms.Closed,
                        CreatedAt = ms.CreatedAt,
                        ClosedAt = ms.ClosedAt,
                        ClosedByUserName = ms.ClosedByUser?.FullName
                    })
                    .ToList();

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

                // Get available years
                var availableYears = await _context.MonthlySettlements
                    .Select(ms => ms.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                var viewModel = new SettlementListViewModel
                {
                    Settlements = settlements,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalSettlements = totalSettlements,
                    UniversityId = universityId,
                    Closed = closed,
                    Year = year,
                    Month = month,
                    TotalSettlementAmount = totalAmount,
                    ClosedAmount = closedAmount,
                    OpenAmount = openAmount,
                    ClosedCount = allSettlements.Count(ms => ms.Closed),
                    OpenCount = allSettlements.Count(ms => !ms.Closed),
                    Universities = universities,
                    AvailableYears = availableYears
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settlements list");
                TempData["Error"] = "An error occurred while loading settlements.";
                return View("Settlements/Index", new SettlementListViewModel());
            }
        }

        public async Task<IActionResult> CreateSettlement()
        {
            try
            {
                var universities = await _context.Universities
                    .Where(u => u.IsActive)
                    .Select(u => new
                    {
                        u.Id,
                        u.NameEnglish,
                        PendingCommissions = _context.Commissions
                            .Where(c => c.UniversityId == u.Id && !c.Settled)
                            .ToList()
                    })
                    .ToListAsync();

                var viewModel = new SettlementCreateViewModel
                {
                    Year = DateTime.Now.Year,
                    Month = DateTime.Now.Month,
                    Universities = universities.Select(u => new UniversityOption
                    {
                        Id = u.Id,
                        Name = u.NameEnglish,
                        PendingCommissionsCount = u.PendingCommissions.Count,
                        PendingCommissionsTotal = u.PendingCommissions.Sum(c => c.AmountEstimated)
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settlement creation form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Settlements));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSettlement(SettlementCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload universities
                    var universities = await _context.Universities
                        .Where(u => u.IsActive)
                        .Select(u => new
                        {
                            u.Id,
                            u.NameEnglish,
                            PendingCommissions = _context.Commissions
                                .Where(c => c.UniversityId == u.Id && !c.Settled)
                                .ToList()
                        })
                        .ToListAsync();

                    model.Universities = universities.Select(u => new UniversityOption
                    {
                        Id = u.Id,
                        Name = u.NameEnglish,
                        PendingCommissionsCount = u.PendingCommissions.Count,
                        PendingCommissionsTotal = u.PendingCommissions.Sum(c => c.AmountEstimated)
                    }).ToList();

                    return View("Settlements/Create", model);
                }

                // Check if settlement already exists for this university/period
                var existingSettlement = await _context.MonthlySettlements
                    .FirstOrDefaultAsync(ms =>
                        ms.UniversityId == model.UniversityId &&
                        ms.Year == model.Year &&
                        ms.Month == model.Month);

                if (existingSettlement != null)
                {
                    TempData["Error"] = $"A settlement for this period already exists (ID: {existingSettlement.Id}).";
                    return RedirectToAction(nameof(CreateSettlement));
                }

                // Get unsettled commissions for this university
                var commissionsToSettle = await _context.Commissions
                    .Include(c => c.Application)
                        .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Where(c => c.UniversityId == model.UniversityId && !c.Settled)
                    .ToListAsync();

                if (!commissionsToSettle.Any())
                {
                    TempData["Warning"] = "No unsettled commissions found for this university.";
                    return RedirectToAction(nameof(CreateSettlement));
                }

                // Create settlement
                var settlement = new MonthlySettlement
                {
                    UniversityId = model.UniversityId,
                    Year = model.Year,
                    Month = model.Month,
                    TotalCommission = commissionsToSettle.Sum(c => c.AmountEstimated),
                    StudentCount = commissionsToSettle.Select(c => c.Application.StudentId).Distinct().Count(),
                    Closed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MonthlySettlements.Add(settlement);
                await _context.SaveChangesAsync();

                // Link commissions to settlement (mark as Settled=true only when settlement is Closed)
                // At this stage the settlement is still open, so only assign the settlement ID
                foreach (var commission in commissionsToSettle)
                {
                    commission.MonthlySettlementId = settlement.Id;
                    commission.Settled = false; // Will be set to true when settlement is Closed
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created settlement ID {settlement.Id} for university {model.UniversityId}, period {model.Year}-{model.Month:D2}");
                TempData["Success"] = $"Settlement created successfully with {commissionsToSettle.Count} commission(s).";
                return RedirectToAction(nameof(SettlementDetails), new { id = settlement.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating settlement");
                ModelState.AddModelError("", "An error occurred while creating the settlement.");
                return View("Settlements/Create", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SettlementDetails(int id)
        {
            try
            {
                var settlement = await _context.MonthlySettlements
                    .Include(ms => ms.University)
                    .Include(ms => ms.ClosedByUser)
                    .FirstOrDefaultAsync(ms => ms.Id == id);

                if (settlement == null)
                {
                    TempData["Error"] = "Settlement not found.";
                    return RedirectToAction(nameof(Settlements));
                }

                var commissions = await _context.Commissions
                    .Include(c => c.Application)
                        .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(c => c.Application.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(c => c.Application.BtecProgram)
                    .Where(c => c.MonthlySettlementId == id)
                    .ToListAsync();

                var viewModel = new SettlementDetailsViewModel
                {
                    Id = settlement.Id,
                    UniversityId = settlement.UniversityId,
                    UniversityName = settlement.University.NameEnglish,
                    UniversityEmail = settlement.University.Email,
                    UniversityPhone = settlement.University.PhoneNumber,
                    Year = settlement.Year,
                    Month = settlement.Month,
                    Period = new DateTime(settlement.Year, settlement.Month, 1).ToString("MMMM yyyy"),
                    TotalCommission = settlement.TotalCommission,
                    StudentCount = settlement.StudentCount,
                    Closed = settlement.Closed,
                    CreatedAt = settlement.CreatedAt,
                    ClosedAt = settlement.ClosedAt,
                    ClosedByUserName = settlement.ClosedByUser?.FullName,
                    Commissions = commissions.Select(c => new CommissionBreakdownItem
                    {
                        CommissionId = c.Id,
                        ApplicationNumber = c.Application.ApplicationNumber ?? $"APP-{c.ApplicationId:D6}",
                        StudentName = c.Application.Student.User.FullName,
                        StudentEmail = c.Application.Student.User.Email,
                        ProgramName = c.Application.UniversityProgram != null
                            ? c.Application.UniversityProgram.Program.NameEnglish
                            : c.Application.BtecProgram.NameEnglish,
                        Degree = c.Application.UniversityProgram != null
                            ? c.Application.UniversityProgram.Program.Degree.ToString()
                            : "BTEC " + c.Application.BtecProgram.Level.ToString(),
                        CommissionMode = c.Mode.ToString(),
                        BaseAmount = c.BaseAmount,
                        Percentage = c.Percentage,
                        CommissionAmount = c.AmountEstimated,
                        ApplicationDate = c.Application.ApplicationDate,
                        CommissionCreatedAt = c.CreatedAt
                    }).ToList(),
                    AverageCommissionPerStudent = commissions.Any() ? commissions.Average(c => c.AmountEstimated) : 0,
                    HighestCommission = commissions.Any() ? commissions.Max(c => c.AmountEstimated) : 0,
                    LowestCommission = commissions.Any() ? commissions.Min(c => c.AmountEstimated) : 0
                };

                // Calculate mode distribution
                viewModel.ModeDistribution = commissions
                    .GroupBy(c => c.Mode.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                viewModel.ModeAmounts = commissions
                    .GroupBy(c => c.Mode.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(c => c.AmountEstimated));

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading settlement details for ID: {id}");
                TempData["Error"] = "An error occurred while loading settlement details.";
                return RedirectToAction(nameof(Settlements));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseSettlement(int id)
        {
            try
            {
                var settlement = await _context.MonthlySettlements.FindAsync(id);
                if (settlement == null)
                {
                    TempData["Error"] = "Settlement not found.";
                    return RedirectToAction(nameof(Settlements));
                }

                if (settlement.Closed)
                {
                    TempData["Warning"] = "This settlement is already closed.";
                    return RedirectToAction(nameof(SettlementDetails), new { id });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                settlement.Closed = true;
                settlement.ClosedAt = DateTime.UtcNow;
                settlement.ClosedByUserId = currentUser.Id;

                // Now that the settlement is being closed, mark its linked commissions as fully Settled
                var linkedCommissions = await _context.Commissions
                    .Where(c => c.MonthlySettlementId == id && !c.Settled)
                    .ToListAsync();
                foreach (var c in linkedCommissions)
                {
                    c.Settled = true;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Settlement {SettlementId} closed by user {UserId} ({Count} commissions marked Settled)",
                    id, currentUser.Id, linkedCommissions.Count);
                TempData["Success"] = "Settlement closed successfully.";
                return RedirectToAction(nameof(SettlementDetails), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing settlement ID: {id}");
                TempData["Error"] = "An error occurred while closing the settlement.";
                return RedirectToAction(nameof(SettlementDetails), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportSettlement(int id)
        {
            try
            {
                var settlement = await _context.MonthlySettlements
                    .Include(ms => ms.University)
                    .FirstOrDefaultAsync(ms => ms.Id == id);

                if (settlement == null)
                {
                    TempData["Error"] = "Settlement not found.";
                    return RedirectToAction(nameof(Settlements));
                }

                var commissions = await _context.Commissions
                    .Include(c => c.Application)
                        .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                    .Include(c => c.Application.UniversityProgram)
                        .ThenInclude(up => up.Program)
                    .Include(c => c.Application.BtecProgram)
                    .Where(c => c.MonthlySettlementId == id)
                    .OrderBy(c => c.Application.Student.User.FullName)
                    .ToListAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Settlement Report");

                // Header information
                worksheet.Cell(1, 1).Value = "Settlement Report";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;

                worksheet.Cell(2, 1).Value = "University:";
                worksheet.Cell(2, 2).Value = settlement.University.NameEnglish;
                worksheet.Cell(3, 1).Value = "Period:";
                worksheet.Cell(3, 2).Value = new DateTime(settlement.Year, settlement.Month, 1).ToString("MMMM yyyy");
                worksheet.Cell(4, 1).Value = "Total Commission:";
                worksheet.Cell(4, 2).Value = settlement.TotalCommission;
                worksheet.Cell(4, 2).Style.NumberFormat.Format = "#,##0.00 JOD";
                worksheet.Cell(5, 1).Value = "Student Count:";
                worksheet.Cell(5, 2).Value = settlement.StudentCount;
                worksheet.Cell(6, 1).Value = "Status:";
                worksheet.Cell(6, 2).Value = settlement.Closed ? "Closed" : "Open";

                // Commission details header
                int startRow = 8;
                worksheet.Cell(startRow, 1).Value = "Application #";
                worksheet.Cell(startRow, 2).Value = "Student Name";
                worksheet.Cell(startRow, 3).Value = "Student Email";
                worksheet.Cell(startRow, 4).Value = "Program";
                worksheet.Cell(startRow, 5).Value = "Commission Mode";
                worksheet.Cell(startRow, 6).Value = "Base Amount";
                worksheet.Cell(startRow, 7).Value = "Percentage";
                worksheet.Cell(startRow, 8).Value = "Commission Amount";

                var headerRange = worksheet.Range(startRow, 1, startRow, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                // Commission data
                int row = startRow + 1;
                foreach (var commission in commissions)
                {
                    worksheet.Cell(row, 1).Value = commission.Application.ApplicationNumber ?? $"APP-{commission.ApplicationId:D6}";
                    worksheet.Cell(row, 2).Value = commission.Application.Student.User.FullName;
                    worksheet.Cell(row, 3).Value = commission.Application.Student.User.Email;
                    worksheet.Cell(row, 4).Value = commission.Application.UniversityProgram != null
                        ? commission.Application.UniversityProgram.Program.NameEnglish
                        : commission.Application.BtecProgram.NameEnglish;
                    worksheet.Cell(row, 5).Value = commission.Mode.ToString();
                    worksheet.Cell(row, 6).Value = commission.BaseAmount;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 7).Value = commission.Percentage;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "0.00%";
                    worksheet.Cell(row, 8).Value = commission.AmountEstimated;
                    worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
                    row++;
                }

                // Total row
                worksheet.Cell(row, 7).Value = "TOTAL:";
                worksheet.Cell(row, 7).Style.Font.Bold = true;
                worksheet.Cell(row, 8).Value = commissions.Sum(c => c.AmountEstimated);
                worksheet.Cell(row, 8).Style.Font.Bold = true;
                worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00 JOD";

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = $"Settlement_{settlement.University.NameEnglish.Replace(" ", "_")}_{settlement.Year}_{settlement.Month:D2}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting settlement ID: {id}");
                TempData["Error"] = "An error occurred while exporting the settlement.";
                return RedirectToAction(nameof(SettlementDetails), new { id });
            }
        }

        #endregion
    }
}
