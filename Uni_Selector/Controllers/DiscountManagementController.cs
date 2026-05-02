using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.DiscountManagement;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "UniversityRep")]
    [Route("UniversityRep/Discounts")]
    public class DiscountManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscountManagementController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /UniversityRep/Discounts
        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, DiscountStatus? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                TempData["Error"] = "You do not have permission to view discounts.";
                return RedirectToAction("Index", "UniversityRep");
            }

            // Query discounts for this university
            var query = _context.DiscountGrants
                .Include(d => d.Application)
                    .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(d => d.RedeemedByUser)
                .Where(d => d.UniversityId == rep.UniversityId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.Code.Contains(search) ||
                    d.Application.ApplicationNumber.Contains(search) ||
                    d.Application.Student.User.FullName.Contains(search) ||
                    d.Application.Student.User.Email.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.GrantedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(d => d.GrantedAt <= endOfDay);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paginated results
            var discounts = await query
                .OrderByDescending(d => d.GrantedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DiscountListItemDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    StudentName = d.Application.Student.User.FullName,
                    StudentEmail = d.Application.Student.User.Email,
                    ApplicationNumber = d.Application.ApplicationNumber,
                    AdmissionNumber = d.Application.AdmissionNumber,
                    Percentage = d.Percentage,
                    AmountEstimated = d.AmountEstimated,
                    Status = d.Status,
                    GrantedAt = d.GrantedAt,
                    RedeemedAt = d.RedeemedAt,
                    RedeemedByUserName = d.RedeemedByUser != null ? d.RedeemedByUser.FullName : null
                })
                .ToListAsync();

            var viewModel = new DiscountListViewModel
            {
                Discounts = discounts,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Search = search,
                StatusFilter = status,
                FromDate = fromDate,
                ToDate = toDate,
                UniversityId = rep.UniversityId,
                UniversityName = rep.University.NameEnglish,
                CanViewDiscounts = rep.CanViewApplications
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/Discounts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                TempData["Error"] = "You do not have permission to view discounts.";
                return RedirectToAction("Index");
            }

            // Load discount with all related data
            var discount = await _context.DiscountGrants
                .Include(d => d.Application)
                    .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(d => d.Application.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(d => d.Application.BtecProgram)
                .Include(d => d.Application.Commission)
                .Include(d => d.University)
                .Include(d => d.RedeemedByUser)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discount == null)
            {
                TempData["Error"] = "Discount not found.";
                return RedirectToAction("Index");
            }

            // Verify discount belongs to rep's university
            if (discount.UniversityId != rep.UniversityId)
            {
                TempData["Error"] = "Discount not found.";
                return RedirectToAction("Index");
            }

            var application = discount.Application;
            var student = application.Student;
            var isBtecProgram = application.BtecProgramId.HasValue;

            var viewModel = new DiscountDetailsViewModel
            {
                // Discount Info
                DiscountId = discount.Id,
                Code = discount.Code,
                Percentage = discount.Percentage,
                AmountEstimated = discount.AmountEstimated,
                Status = discount.Status,
                GrantedAt = discount.GrantedAt,
                RedeemedAt = discount.RedeemedAt,
                RedeemedByUserName = discount.RedeemedByUser?.FullName,

                // Application Info
                ApplicationId = application.Id,
                ApplicationNumber = application.ApplicationNumber,
                AdmissionNumber = application.AdmissionNumber,
                ApplicationStatus = application.Status,
                ApplicationDate = application.ApplicationDate,
                ApprovalDate = application.ApprovalDate,

                // Student Info
                StudentId = student.Id,
                StudentUserId = student.UserId,
                StudentName = student.User.FullName,
                StudentEmail = student.User.Email,
                StudentPhone = student.User.PhoneNumber ?? "",
                NationalId = student.NationalId,
                SeatNumber = student.SeatNumber,
                Gender = student.Gender,
                DateOfBirth = student.DateOfBirth,
                Nationality = student.Nationality,
                GPA = student.GPA,
                Path = student.Path,

                // Program Info
                IsBtecProgram = isBtecProgram,
                ProgramNameEnglish = isBtecProgram ? application.BtecProgram!.NameEnglish : application.UniversityProgram!.Program.NameEnglish,
                ProgramNameArabic = isBtecProgram ? application.BtecProgram!.NameArabic : application.UniversityProgram!.Program.NameArabic,
                ProgramDescription = isBtecProgram ? application.BtecProgram!.Description : application.UniversityProgram!.Program.Description,
                Degree = isBtecProgram ? null : application.UniversityProgram!.Program.Degree,
                BtecLevel = isBtecProgram ? application.BtecProgram!.Level : null,
                ProgramLanguage = isBtecProgram ? application.BtecProgram!.Language : application.UniversityProgram!.Program.Language,
                ProgramDuration = isBtecProgram ? application.BtecProgram!.DurationInYears : application.UniversityProgram!.DurationInYears,
                TotalCreditHours = isBtecProgram ? application.BtecProgram!.TotalCreditHours : application.UniversityProgram!.Program.TotalCreditHours,
                HourPriceBase = isBtecProgram ? application.BtecProgram!.HourPriceBase : application.UniversityProgram!.HourPriceBase,
                RegistrationFeeFirstSemester = isBtecProgram ? application.BtecProgram!.RegistrationFeeFirstSemester : application.UniversityProgram!.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = isBtecProgram ? application.BtecProgram!.RegistrationFeeRegularSemester : application.UniversityProgram!.RegistrationFeeRegularSemester,

                // Commission Info
                HasCommission = application.Commission != null,
                CommissionPercentage = application.Commission?.Percentage,
                CommissionAmountEstimated = application.Commission?.AmountEstimated,
                CommissionMode = application.Commission?.Mode,
                CommissionSettled = application.Commission?.Settled,

                // University Info
                UniversityId = discount.University.Id,
                UniversityNameEnglish = discount.University.NameEnglish,
                UniversityNameArabic = discount.University.NameArabic,
                UniversityCity = discount.University.City,

                // Permissions
                CanRedeem = discount.Status == DiscountStatus.Issued && rep.CanViewApplications
            };

            return View(viewModel);
        }

        // POST: /UniversityRep/Discounts/{id}/Redeem
        [HttpPost("{id}/Redeem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Redeem(int id, [FromForm] RedeemDiscountViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new { success = false, message = "User not authenticated." });

            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                return Json(new { success = false, message = "You do not have permission to redeem discounts." });
            }

            // Load discount
            var discount = await _context.DiscountGrants
                .Include(d => d.Application)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discount == null)
            {
                return Json(new { success = false, message = "Discount not found." });
            }

            // Verify discount belongs to rep's university
            if (discount.UniversityId != rep.UniversityId)
            {
                return Json(new { success = false, message = "Discount not found." });
            }

            // Verify discount can be redeemed
            if (discount.Status != DiscountStatus.Issued)
            {
                return Json(new { success = false, message = $"Discount cannot be redeemed. Current status: {discount.Status}" });
            }

            // Enforce 90-day expiry (consistent with DiscountsController.Verify)
            if (discount.GrantedAt.AddDays(90) < DateTime.UtcNow)
            {
                discount.Status = DiscountStatus.Expired;
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = "Discount has expired (valid for 90 days from grant date)." });
            }

            // Verify application is enrolled
            if (discount.Application.Status != ApplicationStatus.Enrolled)
            {
                return Json(new { success = false, message = "Discount can only be redeemed for enrolled students." });
            }

            // Update discount
            discount.Status = DiscountStatus.Redeemed;
            discount.RedeemedAt = DateTime.UtcNow;
            discount.RedeemedByUserId = currentUser.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Discount {discount.Code} has been successfully redeemed.";
            return Json(new { success = true, message = "Discount redeemed successfully." });
        }

        // GET: /UniversityRep/Discounts/Statistics
        [HttpGet("Statistics")]
        public async Task<IActionResult> Statistics()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var rep = await _context.UniversityRepresentatives
                .Include(r => r.University)
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.IsActive);

            if (rep == null || !rep.CanViewApplications)
            {
                TempData["Error"] = "You do not have permission to view discount statistics.";
                return RedirectToAction("Index", "UniversityRep");
            }

            var universityId = rep.UniversityId;

            // Overview Stats
            var allDiscounts = await _context.DiscountGrants
                .Where(d => d.UniversityId == universityId)
                .ToListAsync();

            var totalIssued = allDiscounts.Count;
            var totalRedeemed = allDiscounts.Count(d => d.Status == DiscountStatus.Redeemed);
            var totalExpired = allDiscounts.Count(d => d.Status == DiscountStatus.Expired);
            var totalPending = allDiscounts.Count(d => d.Status == DiscountStatus.Issued);

            var totalAmountIssued = allDiscounts.Sum(d => d.AmountEstimated);
            var totalAmountRedeemed = allDiscounts.Where(d => d.Status == DiscountStatus.Redeemed).Sum(d => d.AmountEstimated);
            var averagePercentage = allDiscounts.Any() ? allDiscounts.Average(d => d.Percentage) : 0;

            // Status Breakdown
            var discountsByStatus = allDiscounts
                .GroupBy(d => d.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var amountByStatus = allDiscounts
                .GroupBy(d => d.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(d => d.AmountEstimated));

            // Monthly Trends (last 12 months)
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
            var monthlyDiscounts = await _context.DiscountGrants
                .Where(d => d.UniversityId == universityId && d.GrantedAt >= twelveMonthsAgo)
                .ToListAsync();

            var monthlyTrends = monthlyDiscounts
                .GroupBy(d => new { d.GrantedAt.Year, d.GrantedAt.Month })
                .Select(g => new MonthlyDiscountTrend
                {
                    MonthYear = $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                    Issued = g.Count(),
                    Redeemed = g.Count(d => d.Status == DiscountStatus.Redeemed),
                    TotalAmount = g.Sum(d => d.AmountEstimated)
                })
                .OrderBy(m => m.MonthYear)
                .ToList();

            // Program Distribution (top 10)
            var programStats = await _context.DiscountGrants
                .Include(d => d.Application.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(d => d.Application.BtecProgram)
                .Where(d => d.UniversityId == universityId)
                .ToListAsync();

            var programDistribution = programStats
                .GroupBy(d => d.Application.BtecProgramId.HasValue
                    ? d.Application.BtecProgram!.NameEnglish
                    : d.Application.UniversityProgram!.Program.NameEnglish)
                .Select(g => new ProgramDiscountStats
                {
                    ProgramName = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(d => d.AmountEstimated),
                    RedeemedCount = g.Count(d => d.Status == DiscountStatus.Redeemed)
                })
                .OrderByDescending(p => p.Count)
                .Take(10)
                .ToList();

            // Recent Activity (last 10)
            var recentDiscounts = await _context.DiscountGrants
                .Include(d => d.Application)
                    .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(d => d.RedeemedByUser)
                .Where(d => d.UniversityId == universityId)
                .OrderByDescending(d => d.GrantedAt)
                .Take(10)
                .Select(d => new DiscountListItemDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    StudentName = d.Application.Student.User.FullName,
                    StudentEmail = d.Application.Student.User.Email,
                    ApplicationNumber = d.Application.ApplicationNumber,
                    AdmissionNumber = d.Application.AdmissionNumber,
                    Percentage = d.Percentage,
                    AmountEstimated = d.AmountEstimated,
                    Status = d.Status,
                    GrantedAt = d.GrantedAt,
                    RedeemedAt = d.RedeemedAt,
                    RedeemedByUserName = d.RedeemedByUser != null ? d.RedeemedByUser.FullName : null
                })
                .ToListAsync();

            var viewModel = new DiscountStatisticsViewModel
            {
                TotalDiscountsIssued = totalIssued,
                TotalDiscountsRedeemed = totalRedeemed,
                TotalDiscountsExpired = totalExpired,
                TotalDiscountsPending = totalPending,
                TotalAmountIssued = totalAmountIssued,
                TotalAmountRedeemed = totalAmountRedeemed,
                AverageDiscountPercentage = averagePercentage,
                DiscountsByStatus = discountsByStatus,
                AmountByStatus = amountByStatus,
                MonthlyTrends = monthlyTrends,
                ProgramStats = programDistribution,
                RecentDiscounts = recentDiscounts,
                UniversityId = universityId,
                UniversityName = rep.University.NameEnglish
            };

            return View(viewModel);
        }
    }
}