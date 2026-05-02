using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Uni_Selector.Data;
using Uni_Selector.Helpers;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.Admin;

namespace Uni_Selector.Controllers
{


    [Authorize(Roles = UserRoles.PlatformAdmin)]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var viewModel = new AdminDashboardViewModel
                {
                    // Total Counts
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalUniversities = await _context.Universities.CountAsync(),
                    TotalPrograms = await _context.UniversityPrograms.CountAsync() + await _context.BtecPrograms.CountAsync(),
                    TotalApplications = await _context.StudentApplications.CountAsync(),

                    // Active Counts
                    ActiveUniversities = await _context.Universities.CountAsync(u => u.IsActive),
                    ActivePrograms = await _context.UniversityPrograms.CountAsync(p => p.IsActive) +
                                    await _context.BtecPrograms.CountAsync(b => b.IsActive),

                    // Application Statistics
                    PendingApplications = await _context.StudentApplications
                        .CountAsync(a => a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.UnderReview),
                    ApprovedApplications = await _context.StudentApplications
                        .CountAsync(a => a.Status == ApplicationStatus.Approved),
                    RejectedApplications = await _context.StudentApplications
                        .CountAsync(a => a.Status == ApplicationStatus.Rejected),
                    EnrolledApplications = await _context.StudentApplications
                        .CountAsync(a => a.Status == ApplicationStatus.Enrolled),

                    // Financial Statistics
                    TotalCommissionsGenerated = await _context.Commissions
                        .Where(c => c.Settled)
                        .SumAsync(c => (decimal?)c.AmountEstimated) ?? 0,
                    PendingCommissions = await _context.Commissions
                        .Where(c => !c.Settled)
                        .SumAsync(c => (decimal?)c.AmountEstimated) ?? 0,
                    TotalDiscountsIssued = await _context.DiscountGrants.CountAsync(),
                    TotalDiscountsRedeemed = await _context.DiscountGrants
                        .CountAsync(d => d.Status == DiscountStatus.Redeemed),

                    // Recent Activity (Last 30 days)
                    NewUsersLast30Days = await _context.Users
                        .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                    NewApplicationsLast30Days = await _context.StudentApplications
                        .CountAsync(a => a.ApplicationDate >= DateTime.UtcNow.AddDays(-30)),
                    NewUniversitiesLast30Days = await _context.Universities
                        .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)),

                    // Recent Applications (Latest 10)
                    RecentApplications = await _context.StudentApplications
                        .Include(a => a.Student)
                            .ThenInclude(s => s.User)
                        .Include(a => a.UniversityProgram)
                            .ThenInclude(up => up.University)
                        .Include(a => a.UniversityProgram)
                            .ThenInclude(up => up.Program)
                        .Include(a => a.BtecProgram)
                            .ThenInclude(bp => bp.University)
                        .OrderByDescending(a => a.ApplicationDate)
                        .Take(10)
                        .Select(a => new ApplicationSummaryDto
                        {
                            Id = a.Id,
                            ApplicationNumber = a.ApplicationNumber,
                            StudentName = a.Student.User.FullName,
                            UniversityName = a.UniversityProgram != null
                                ? a.UniversityProgram.University.NameEnglish
                                : a.BtecProgram.University.NameEnglish,
                            ProgramName = a.UniversityProgram != null
                                ? a.UniversityProgram.Program.NameEnglish
                                : a.BtecProgram.NameEnglish,
                            Status = a.Status,
                            ApplicationDate = a.ApplicationDate
                        })
                        .ToListAsync(),

                    // Recent Users (Latest 10)
                    RecentUsers = await _context.Users
                        .OrderByDescending(u => u.CreatedAt)
                        .Take(10)
                        .Select(u => new UserSummaryDto
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            Email = u.Email,
                            IsActive = u.IsActive,
                            CreatedAt = u.CreatedAt,
                            LastLoginAt = u.LastLoginAt
                        })
                        .ToListAsync(),

                    // University Statistics
                    UniversityStats = await GetUniversityStatsAsync(),

                    // Application Status Distribution
                    ApplicationsByStatus = await _context.StudentApplications
                        .GroupBy(a => a.Status)
                        .Select(g => new StatusCountDto
                        {
                            Status = g.Key,
                            Count = g.Count()
                        })
                        .ToListAsync(),

                    // Monthly Applications (Last 12 months)
                    MonthlyApplications = await GetMonthlyApplicationStats(),

                    // System Health Indicators
                    UnreadNotifications = await _context.Notifications
                        .CountAsync(n => !n.IsRead),
                    PendingBtecApprovals = await _context.BtecPrograms
                        .CountAsync(b => !b.IsApprovedByBtecAuthority && b.IsActive)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading dashboard data. Please try again.";
                return View(new AdminDashboardViewModel());
            }
        }






        public async Task<IActionResult> SystemHealth()
        {
            try
            {
                // Get response time statistics
                var stats = ResponseTimeTracker.GetStatistics();

                var viewModel = new SystemHealthViewModel
                {
                    // Database Health
                    DatabaseStatus = await CheckDatabaseHealth(),
                    TotalDatabaseSize = await GetDatabaseSize(),

                    // System Metrics
                    TotalUsers = await _context.Users.CountAsync(),
                    ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                    InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
                    UsersLoggedInLast24Hours = await _context.Users
                        .CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddHours(-24)),

                    // Application Metrics
                    TotalApplications = await _context.StudentApplications.CountAsync(),
                    ApplicationsLast24Hours = await _context.StudentApplications
                        .CountAsync(a => a.ApplicationDate >= DateTime.UtcNow.AddHours(-24)),
                    ApplicationsLast7Days = await _context.StudentApplications
                        .CountAsync(a => a.ApplicationDate >= DateTime.UtcNow.AddDays(-7)),
                    ApplicationsLast30Days = await _context.StudentApplications
                        .CountAsync(a => a.ApplicationDate >= DateTime.UtcNow.AddDays(-30)),

                    // Notification Metrics
                    TotalNotifications = await _context.Notifications.CountAsync(),
                    UnreadNotifications = await _context.Notifications.CountAsync(n => !n.IsRead),
                    NotificationsSent24Hours = await _context.Notifications
                        .CountAsync(n => n.CreatedAt >= DateTime.UtcNow.AddHours(-24)),
                    FailedEmailNotifications = await _context.Notifications
                        .CountAsync(n => n.Channel == NotificationChannel.Email && !n.EmailSent),

                    // Commission & Financial Metrics
                    TotalCommissions = await _context.Commissions.CountAsync(),
                    SettledCommissions = await _context.Commissions.CountAsync(c => c.Settled),
                    PendingCommissions = await _context.Commissions.CountAsync(c => !c.Settled),
                    TotalCommissionAmount = await _context.Commissions
                        .SumAsync(c => (decimal?)c.AmountEstimated) ?? 0,
                    SettledCommissionAmount = await _context.Commissions
                        .Where(c => c.Settled)
                        .SumAsync(c => (decimal?)c.AmountEstimated) ?? 0,

                    // Discount Metrics
                    TotalDiscounts = await _context.DiscountGrants.CountAsync(),
                    IssuedDiscounts = await _context.DiscountGrants
                        .CountAsync(d => d.Status == DiscountStatus.Issued),
                    RedeemedDiscounts = await _context.DiscountGrants
                        .CountAsync(d => d.Status == DiscountStatus.Redeemed),
                    ExpiredDiscounts = await _context.DiscountGrants
                        .CountAsync(d => d.Status == DiscountStatus.Expired),

                    // University & Program Metrics
                    TotalUniversities = await _context.Universities.CountAsync(),
                    ActiveUniversities = await _context.Universities.CountAsync(u => u.IsActive),
                    TotalPrograms = await _context.UniversityPrograms.CountAsync() +
                                   await _context.BtecPrograms.CountAsync(),
                    ActivePrograms = await _context.UniversityPrograms.CountAsync(p => p.IsActive) +
                                    await _context.BtecPrograms.CountAsync(b => b.IsActive),
                    PendingBtecApprovals = await _context.BtecPrograms
                        .CountAsync(b => !b.IsApprovedByBtecAuthority && b.IsActive),

                    // Recommendation Metrics
                    TotalRecommendations = await _context.Recommendations.CountAsync(),
                    RecommendationsLast24Hours = await _context.Recommendations
                        .CountAsync(r => r.CreatedAt >= DateTime.UtcNow.AddHours(-24)),
                    ViewedRecommendations = await _context.Recommendations.CountAsync(r => r.IsViewed),

                    // Performance Metrics
                    AverageResponseTime = GetAverageResponseTime(),
                    ServerUptime = GetServerUptime(),
                    MemoryUsage = GetMemoryUsage(),
                    CpuUsage = GetCpuUsageHelper(),

                    // Response Time Details
                    MinResponseTime = stats.MinResponseTime,
                    MaxResponseTime = stats.MaxResponseTime,
                    MedianResponseTime = stats.MedianResponseTime,
                    Last1MinuteResponseTime = stats.Last1MinuteAverage,
                    Last5MinutesResponseTime = stats.Last5MinutesAverage,
                    RequestsPerMinute = stats.RequestsLastMinute,
                    SuccessRate = stats.SuccessRate,

                    // Error & Log Metrics
                    ErrorsLast24Hours = 0, // Implement based on your logging system
                    WarningsLast24Hours = 0, // Implement based on your logging system

                    // Last Update Time
                    LastHealthCheckTime = DateTime.UtcNow
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading system health data. Please try again.";
                return View(new SystemHealthViewModel());
            }
        }

        #region Helper Methods

        private async Task<List<UniversityStatsDto>> GetUniversityStatsAsync()
        {
            var universities = await _context.Universities
                .Where(u => u.IsActive)
                .ToListAsync();

            var stats = new List<UniversityStatsDto>();

            foreach (var university in universities)
            {
                // Count programs
                var programCount = await _context.UniversityPrograms
                    .CountAsync(p => p.UniversityId == university.Id);

                var btecProgramCount = await _context.BtecPrograms
                    .CountAsync(b => b.UniversityId == university.Id);

                // Count applications (through both UniversityProgram and BtecProgram)
                var regularAppCount = await _context.StudentApplications
                    .Where(a => a.UniversityProgram != null && a.UniversityProgram.UniversityId == university.Id)
                    .CountAsync();

                var btecAppCount = await _context.StudentApplications
                    .Where(a => a.BtecProgram != null && a.BtecProgram.UniversityId == university.Id)
                    .CountAsync();

                var totalApplications = regularAppCount + btecAppCount;

                // Count approved applications
                var regularApprovedCount = await _context.StudentApplications
                    .Where(a => a.UniversityProgram != null &&
                               a.UniversityProgram.UniversityId == university.Id &&
                               a.Status == ApplicationStatus.Approved)
                    .CountAsync();

                var btecApprovedCount = await _context.StudentApplications
                    .Where(a => a.BtecProgram != null &&
                               a.BtecProgram.UniversityId == university.Id &&
                               a.Status == ApplicationStatus.Approved)
                    .CountAsync();

                var approvedApplications = regularApprovedCount + btecApprovedCount;

                stats.Add(new UniversityStatsDto
                {
                    UniversityName = university.NameEnglish,
                    TotalPrograms = programCount + btecProgramCount,
                    TotalApplications = totalApplications,
                    ApprovedApplications = approvedApplications
                });
            }

            return stats.OrderByDescending(s => s.TotalApplications).Take(5).ToList();
        }

        private async Task<List<MonthlyStatDto>> GetMonthlyApplicationStats()
        {
            var last12Months = new List<MonthlyStatDto>();
            var startDate = DateTime.UtcNow.AddMonths(-12);

            for (int i = 0; i < 12; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var count = await _context.StudentApplications
                    .CountAsync(a => a.ApplicationDate >= monthStart && a.ApplicationDate < monthEnd);

                last12Months.Add(new MonthlyStatDto
                {
                    Month = monthStart.ToString("MMM yyyy"),
                    Count = count
                });
            }

            return last12Months;
        }

        private async Task<string> CheckDatabaseHealth()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return "Healthy";
            }
            catch
            {
                return "Unhealthy";
            }
        }

        private async Task<long> GetDatabaseSize()
        {
            try
            {
                var totalRecords = await _context.Users.CountAsync() +
                                  await _context.StudentApplications.CountAsync() +
                                  await _context.Universities.CountAsync() +
                                  await _context.Notifications.CountAsync();

                return totalRecords * 1024;
            }
            catch
            {
                return 0;
            }
        }

        private double GetAverageResponseTime()
        {
            return ResponseTimeTracker.GetAverageResponseTime();
        }

        private TimeSpan GetServerUptime()
        {
            using var process = Process.GetCurrentProcess();
            return DateTime.Now - process.StartTime;
        }

        private double GetMemoryUsage()
        {
            using var process = Process.GetCurrentProcess();
            return process.WorkingSet64 / (1024.0 * 1024.0); // Convert to MB
        }

        private double GetCpuUsageHelper()
        {
            return CpuUsageHelper.GetCurrentCpuUsage();
        }
        #endregion

        #region API Endpoints for Real-Time Monitoring
        [HttpGet]
        public IActionResult GetCpuUsage()
        {
            return Json(new
            {
                cpuUsage = CpuUsageHelper.GetCurrentCpuUsage(),
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet]
        public IActionResult GetResponseTimeStats()
        {
            var stats = ResponseTimeTracker.GetStatistics();
            var percentiles = ResponseTimeTracker.GetPercentiles();

            return Json(new
            {
                average = stats.AverageResponseTime,
                min = stats.MinResponseTime,
                max = stats.MaxResponseTime,
                median = stats.MedianResponseTime,
                last1Min = stats.Last1MinuteAverage,
                last5Min = stats.Last5MinutesAverage,
                percentiles = new
                {
                    p50 = percentiles.P50,
                    p95 = percentiles.P95,
                    p99 = percentiles.P99
                },
                totalRequests = stats.TotalRequests,
                requestsPerMinute = stats.RequestsLastMinute,
                successRate = stats.SuccessRate,
                performanceStatus = stats.PerformanceStatus,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet]
        public IActionResult GetSlowestEndpoints()
        {
            var endpoints = ResponseTimeTracker.GetSlowestEndpoints(10);
            return Json(endpoints);
        }
        #endregion


    }

}





