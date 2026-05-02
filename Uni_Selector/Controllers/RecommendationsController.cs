using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service;
using Uni_Selector.ViewModels.Recommendations;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles =UserRoles.Student)]
    [Route("Recommendations")]
    public class RecommendationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecommendationService _recommendationService;

        public RecommendationsController(AppDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RecommendationService recommendationService)
        {
            _context = context;
            _userManager = userManager;
            _recommendationService = recommendationService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchTerm, string universityFilter, string languageFilter,
              string sortBy = "score", int pageNumber = 1, int pageSize = 12)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null || student.ProfileCompleted != true)
            {
                TempData["Info"] = "Please complete your profile first to get recommendations.";
                return RedirectToAction("CompleteProfile", "Student");
            }

            // Get recommendations query
            var query = _context.Recommendations
                .Include(r => r.UniversityProgram)
                    .ThenInclude(up => up.University)
                .Include(r => r.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(r => r.BtecProgram)
                    .ThenInclude(bp => bp.University)
                .Where(r => r.StudentId == student.Id);

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    (r.UniversityProgram != null && r.UniversityProgram.Program.NameEnglish.Contains(searchTerm)) ||
                    (r.BtecProgram != null && r.BtecProgram.NameEnglish.Contains(searchTerm)) ||
                    (r.UniversityProgram != null && r.UniversityProgram.University.NameEnglish.Contains(searchTerm)) ||
                    (r.BtecProgram != null && r.BtecProgram.University.NameEnglish.Contains(searchTerm))
                );
            }

            if (!string.IsNullOrEmpty(universityFilter))
            {
                query = query.Where(r =>
                    (r.UniversityProgram != null && r.UniversityProgram.University.NameEnglish.Contains(universityFilter)) ||
                    (r.BtecProgram != null && r.BtecProgram.University.NameEnglish.Contains(universityFilter))
                );
            }

            if (!string.IsNullOrEmpty(languageFilter) && Enum.TryParse<LanguageCode>(languageFilter, out var langCode))
            {
                query = query.Where(r =>
                    (r.UniversityProgram != null && r.UniversityProgram.Program.Language == langCode) ||
                    (r.BtecProgram != null && r.BtecProgram.Language == langCode)
                );
            }

// Linq 

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "cost" => query.OrderBy(r => r.EstimatedTotalCost),
                "distance" => query.OrderBy(r => r.DistanceInKm),
                "newest" => query.OrderByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.Score), // Default: score
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var recommendations = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get applications for these programs
            var programIds = recommendations.Where(r => r.UniversityProgramId.HasValue).Select(r => r.UniversityProgramId.Value).ToList();
            var btecIds = recommendations.Where(r => r.BtecProgramId.HasValue).Select(r => r.BtecProgramId.Value).ToList();

            var applications = await _context.StudentApplications
                .Where(a => a.StudentId == student.Id &&
                    ((a.UniversityProgramId.HasValue && programIds.Contains(a.UniversityProgramId.Value)) ||
                     (a.BtecProgramId.HasValue && btecIds.Contains(a.BtecProgramId.Value))))
                .ToListAsync();

            var model = new RecommendationsListViewModel
            {
                Recommendations = recommendations.Select(r => new RecommendationItemViewModel
                {
                    Id = r.Id,
                    ProgramName = r.UniversityProgram != null ? r.UniversityProgram.Program.NameEnglish : r.BtecProgram.NameEnglish,
                    ProgramNameArabic = r.UniversityProgram != null ? r.UniversityProgram.Program.NameArabic : r.BtecProgram.NameArabic,
                    UniversityName = r.UniversityProgram != null ? r.UniversityProgram.University.NameEnglish : r.BtecProgram.University.NameEnglish,
                    UniversityLogo = r.UniversityProgram != null ? r.UniversityProgram.University.LogoPath : r.BtecProgram.University.LogoPath,
                    Score = r.Score,
                    EstimatedCost = r.EstimatedTotalCost,
                    DistanceKm = r.DistanceInKm,
                    Language = r.UniversityProgram != null ? r.UniversityProgram.Program.Language.ToString() : r.BtecProgram.Language.ToString(),
                    IsBtec = r.BtecProgramId.HasValue,
                    IsViewed = r.IsViewed,
                    // Branch by type to avoid null-OR false positives across different program types
                    HasApplication = r.BtecProgramId.HasValue
                        ? applications.Any(a => a.BtecProgramId == r.BtecProgramId)
                        : applications.Any(a => a.UniversityProgramId == r.UniversityProgramId),
                    ApplicationStatus = (r.BtecProgramId.HasValue
                        ? applications.FirstOrDefault(a => a.BtecProgramId == r.BtecProgramId)
                        : applications.FirstOrDefault(a => a.UniversityProgramId == r.UniversityProgramId))?.Status.ToString()
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                UniversityFilter = universityFilter,
                LanguageFilter = languageFilter,
                SortBy = sortBy
            };

            ViewBag.HasRecommendations = recommendations.Any();
            return View(model);
        }



        [HttpGet("Generate")]
        public async Task<IActionResult> Generate()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null || student.ProfileCompleted != true)
            {
                TempData["Error"] = "Please complete your profile first.";
                return RedirectToAction("CompleteProfile", "Student");
            }

            // Check for existing recommendations
            var existingCount = await _context.Recommendations
                .Where(r => r.StudentId == student.Id)
                .CountAsync();

            var model = new GenerateRecommendationsViewModel
            {
                HasExistingRecommendations = existingCount > 0,
                ExistingRecommendationsCount = existingCount
            };

            return View(model);
        }

        [HttpPost("Generate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(bool regenerate = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null || student.ProfileCompleted != true)
            {
                TempData["Error"] = "Please complete your profile first.";
                return RedirectToAction("CompleteProfile", "Student");
            }

            try
            {
                // Generate recommendations
                var recommendations = await _recommendationService.GenerateRecommendationsAsync(student.Id);

                if (recommendations.Any())
                {
                    TempData["Success"] = $"Successfully generated {recommendations.Count} recommendations based on your profile!";
                }
                else
                {
                    TempData["Warning"] = "No matching programs found based on your profile. Please update your preferences or contact support.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while generating recommendations. Please try again.";
                return RedirectToAction(nameof(Generate));
            }
        }

        [HttpGet("{id}/Details")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            var recommendation = await _context.Recommendations
                .Include(r => r.UniversityProgram)
                    .ThenInclude(up => up.University)
                .Include(r => r.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(r => r.UniversityProgram)
                    .ThenInclude(up => up.EntryRequirements)
                .Include(r => r.BtecProgram)
                    .ThenInclude(bp => bp.University)
                .Include(r => r.BtecProgram)
                    .ThenInclude(bp => bp.EntryRequirements)
                .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == student.Id);

            if (recommendation == null)
                return NotFound();

            // Deserialize reasons
            List<string> reasons = new List<string>();
            if (!string.IsNullOrEmpty(recommendation.ReasonsJson))
            {
                try
                {
                    reasons = System.Text.Json.JsonSerializer.Deserialize<List<string>>(recommendation.ReasonsJson);
                }
                catch { }
            }

            // Check if student has application for this program — branch by type to avoid null-OR false positives
            bool hasApplication;
            StudentApplication? application;
            if (recommendation.BtecProgramId.HasValue)
            {
                hasApplication = await _context.StudentApplications
                    .AnyAsync(a => a.StudentId == student.Id && a.BtecProgramId == recommendation.BtecProgramId);
                application = await _context.StudentApplications
                    .FirstOrDefaultAsync(a => a.StudentId == student.Id && a.BtecProgramId == recommendation.BtecProgramId);
            }
            else
            {
                hasApplication = await _context.StudentApplications
                    .AnyAsync(a => a.StudentId == student.Id && a.UniversityProgramId == recommendation.UniversityProgramId);
                application = await _context.StudentApplications
                    .FirstOrDefaultAsync(a => a.StudentId == student.Id && a.UniversityProgramId == recommendation.UniversityProgramId);
            }

            var model = new RecommendationDetailsViewModel
            {
                RecommendationId = recommendation.Id,
                Score = recommendation.Score,
                EstimatedTotalCost = recommendation.EstimatedTotalCost,
                DistanceInKm = recommendation.DistanceInKm,
                Reasons = reasons,
                IsBtec = recommendation.BtecProgramId.HasValue,
                HasApplication = hasApplication,
                ApplicationStatus = application?.Status.ToString(),
                ApplicationDate = application?.ApplicationDate
            };

            if (recommendation.UniversityProgram != null)
            {
                model.ProgramName = recommendation.UniversityProgram.Program.NameEnglish;
                model.ProgramNameArabic = recommendation.UniversityProgram.Program.NameArabic;
                model.Description = recommendation.UniversityProgram.Program.Description;
                model.UniversityName = recommendation.UniversityProgram.University.NameEnglish;
                model.UniversityLogo = recommendation.UniversityProgram.University.LogoPath;
                model.UniversityImage = recommendation.UniversityProgram.University.ImagePath;
                model.UniversityAddress = recommendation.UniversityProgram.University.FullAddress;
                model.UniversityPhone = recommendation.UniversityProgram.University.PhoneNumber;
                model.UniversityEmail = recommendation.UniversityProgram.University.Email;
                model.Language = recommendation.UniversityProgram.Program.Language.ToString();
                model.TotalCreditHours = recommendation.UniversityProgram.Program.TotalCreditHours;
                model.DurationYears = recommendation.UniversityProgram.DurationInYears;
                model.HourPriceBase = recommendation.UniversityProgram.HourPriceBase;
                model.RegistrationFeeFirstSemester = recommendation.UniversityProgram.RegistrationFeeFirstSemester;
                model.RegistrationFeeRegularSemester = recommendation.UniversityProgram.RegistrationFeeRegularSemester;
                model.StudySystem = recommendation.UniversityProgram.StudySystem.ToString();
                model.SemesterStartDate = recommendation.UniversityProgram.SemesterStartDate;
                model.Capacity = recommendation.UniversityProgram.Capacity;
                model.Degree = recommendation.UniversityProgram.Program.Degree.ToString();

                // Entry requirements
                if (recommendation.UniversityProgram.EntryRequirements != null && recommendation.UniversityProgram.EntryRequirements.Any())
                {
                    model.MinGPA = recommendation.UniversityProgram.EntryRequirements.Min(r => r.MinGPA);
                    model.RequiredPath = recommendation.UniversityProgram.EntryRequirements.First().Path.ToString();
                }
            }
            else if (recommendation.BtecProgram != null)
            {
                model.ProgramName = recommendation.BtecProgram.NameEnglish;
                model.ProgramNameArabic = recommendation.BtecProgram.NameArabic;
                model.Description = recommendation.BtecProgram.Description;
                model.UniversityName = recommendation.BtecProgram.University.NameEnglish;
                model.UniversityLogo = recommendation.BtecProgram.University.LogoPath;
                model.UniversityImage = recommendation.BtecProgram.University.ImagePath;
                model.UniversityAddress = recommendation.BtecProgram.University.FullAddress;
                model.UniversityPhone = recommendation.BtecProgram.University.PhoneNumber;
                model.UniversityEmail = recommendation.BtecProgram.University.Email;
                model.Language = recommendation.BtecProgram.Language.ToString();
                model.TotalCreditHours = recommendation.BtecProgram.TotalCreditHours;
                model.DurationYears = recommendation.BtecProgram.DurationInYears;
                model.HourPriceBase = recommendation.BtecProgram.HourPriceBase;
                model.RegistrationFeeFirstSemester = recommendation.BtecProgram.RegistrationFeeFirstSemester;
                model.RegistrationFeeRegularSemester = recommendation.BtecProgram.RegistrationFeeRegularSemester;
                model.SemesterStartDate = recommendation.BtecProgram.SemesterStartDate;
                model.Capacity = recommendation.BtecProgram.Capacity;
                model.BtecLevel = recommendation.BtecProgram.Level.ToString();
                model.TechnicalField = recommendation.BtecProgram.TechnicalField;

                if (recommendation.BtecProgram.EntryRequirements != null && recommendation.BtecProgram.EntryRequirements.Any())
                {
                    model.MinGPA = recommendation.BtecProgram.EntryRequirements.Min(r => r.MinGPA);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsViewed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return Json(new { success = false, message = "Student not found" });

            var recommendation = await _context.Recommendations
                .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == student.Id);

            if (recommendation == null)
                return Json(new { success = false, message = "Recommendation not found" });

            if (!recommendation.IsViewed)
            {
                recommendation.IsViewed = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            var recommendations = await _context.Recommendations
                .Include(r => r.UniversityProgram)
                    .ThenInclude(up => up.University)
                .Include(r => r.UniversityProgram)
                    .ThenInclude(up => up.Program)
                .Include(r => r.BtecProgram)
                    .ThenInclude(bp => bp.University)
                .Where(r => r.StudentId == student.Id)
                .OrderByDescending(r => r.Score)
                .ToListAsync();

            if (!recommendations.Any())
            {
                TempData["Info"] = "No recommendations available to export.";
                return RedirectToAction(nameof(Index));
            }

            // Generate PDF
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                Paragraph title = new Paragraph("University Recommendations Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                document.Add(title);

                // Student Info
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                Paragraph studentInfo = new Paragraph();
                studentInfo.Add(new Chunk("Student Name: ", headerFont));
                studentInfo.Add(new Chunk($"{student.User.FullName}\n", normalFont));
                studentInfo.Add(new Chunk("Email: ", headerFont));
                studentInfo.Add(new Chunk($"{student.User.Email}\n", normalFont));
                studentInfo.Add(new Chunk("GPA: ", headerFont));
                studentInfo.Add(new Chunk($"{student.GPA}\n", normalFont));
                studentInfo.Add(new Chunk("Generated Date: ", headerFont));
                studentInfo.Add(new Chunk($"{DateTime.Now:dd/MM/yyyy}\n", normalFont));
                studentInfo.SpacingAfter = 15f;
                document.Add(studentInfo);

                // Recommendations Table
                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 3f, 1.5f, 1.5f, 1.5f });

                // Table Headers
                Font tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                PdfPCell[] headers = {
                    new PdfPCell(new Phrase("Program", tableHeaderFont)),
                    new PdfPCell(new Phrase("University", tableHeaderFont)),
                    new PdfPCell(new Phrase("Score", tableHeaderFont)),
                    new PdfPCell(new Phrase("Cost (JOD)", tableHeaderFont)),
                    new PdfPCell(new Phrase("Distance", tableHeaderFont))
                };

                foreach (var header in headers)
                {
                    header.BackgroundColor = new BaseColor(52, 152, 219);
                    header.HorizontalAlignment = Element.ALIGN_CENTER;
                    header.Padding = 8f;
                    table.AddCell(header);
                }

                // Table Data
                Font tableCellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                foreach (var rec in recommendations.Take(20)) // Limit to top 20
                {
                    string programName = rec.UniversityProgram != null ? rec.UniversityProgram.Program.NameEnglish : rec.BtecProgram.NameEnglish;
                    string universityName = rec.UniversityProgram != null ? rec.UniversityProgram.University.NameEnglish : rec.BtecProgram.University.NameEnglish;

                    table.AddCell(new PdfPCell(new Phrase(programName, tableCellFont)) { Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase(universityName, tableCellFont)) { Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase($"{rec.Score:F1}", tableCellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase($"{rec.EstimatedTotalCost:F2}", tableCellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase(rec.DistanceInKm.HasValue ? $"{rec.DistanceInKm:F1} km" : "N/A", tableCellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                }

                document.Add(table);

                // Footer
                Paragraph footer = new Paragraph("\nThis report is generated by Uni_Selector Platform",
                    FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.GRAY));
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 20f;
                document.Add(footer);

                document.Close();
                writer.Close();

                byte[] bytes = ms.ToArray();
                return File(bytes, "application/pdf", $"Recommendations_{student.User.FullName}_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }


    }
}
