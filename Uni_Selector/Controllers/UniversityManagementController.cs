using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.ViewModels.UniversityManagement;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "UniversityRep")]
    [Route("UniversityRep/University")]
    public class UniversityManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UniversityManagementController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UniversityManagementController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<UniversityManagementController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        #region View University Details
        // GET: /UniversityRep/University
        [HttpGet("")]
        public async Task<IActionResult> Index()
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
                    return RedirectToAction("Dashboard", "UniversityRep");
                }

                var university = await _context.Universities
                    .Include(u => u.Representatives)
                        .ThenInclude(r => r.User)
                    .Include(u => u.UniversityPrograms)
                        .ThenInclude(up => up.Program)
                    .Include(u => u.BtecPrograms)
                    .FirstOrDefaultAsync(u => u.Id == rep.UniversityId);

                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction("Dashboard", "UniversityRep");
                }

                // Get application statistics
                var applicationStats = await _context.StudentApplications
                    .Include(a => a.UniversityProgram)
                    .Include(a => a.BtecProgram)
                    .Where(a => (a.UniversityProgram != null && a.UniversityProgram.UniversityId == university.Id) ||
                                (a.BtecProgram != null && a.BtecProgram.UniversityId == university.Id))
                    .GroupBy(a => a.Status)
                    .Select(g => new ApplicationStatusStatDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToListAsync();

                var viewModel = new UniversityDetailsViewModel
                {
                    Id = university.Id,
                    NameEnglish = university.NameEnglish,
                    NameArabic = university.NameArabic,
                    Type = university.Type.ToString(),
                    Province = university.Province,
                    City = university.City,
                    FullAddress = university.FullAddress,
                    Latitude = university.Latitude,
                    Longitude = university.Longitude,
                    AcademicAccreditation = university.AcademicAccreditation,
                    PhoneNumber = university.PhoneNumber,
                    Email = university.Email,
                    OfficialWebsite = university.OfficialWebsite,
                    LogoPath = university.LogoPath,
                    ImagePath = university.ImagePath,
                    IsActive = university.IsActive,
                    CommissionMode = university.CommissionMode.ToString(),
                    CreatedAt = university.CreatedAt,
                    UpdatedAt = university.UpdatedAt,

                    // Statistics
                    TotalPrograms = university.UniversityPrograms.Count + university.BtecPrograms.Count,
                    ActivePrograms = university.UniversityPrograms.Count(p => p.IsActive) +
                                    university.BtecPrograms.Count(p => p.IsActive && p.IsApprovedByBtecAuthority),
                    TotalRepresentatives = university.Representatives.Count(r => r.IsActive),
                    ApplicationStats = applicationStats,

                    // Programs
                    UniversityPrograms = university.UniversityPrograms
                        .OrderByDescending(p => p.IsActive)
                        .ThenBy(p => p.Program.NameEnglish)
                        .Select(up => new UniversityProgramInfoDto
                        {
                            Id = up.Id,
                            ProgramNameEnglish = up.Program?.NameEnglish ?? "Unknown",
                            ProgramNameArabic = up.Program?.NameArabic ?? "غير معروف",
                            Degree = up.Program?.Degree.ToString() ?? "N/A",
                            DurationInYears = up.DurationInYears,
                            HourPriceBase = up.HourPriceBase,
                            RegistrationFeeFirstSemester = up.RegistrationFeeFirstSemester,
                            Capacity = up.Capacity,
                            IsActive = up.IsActive
                        }).ToList(),

                    BtecPrograms = university.BtecPrograms
                        .OrderByDescending(p => p.IsActive)
                        .ThenBy(p => p.NameEnglish)
                        .Select(bp => new BtecProgramInfoDto
                        {
                            Id = bp.Id,
                            NameEnglish = bp.NameEnglish,
                            NameArabic = bp.NameArabic,
                            Level = bp.Level.ToString(),
                            TechnicalField = bp.TechnicalField,
                            HourPriceBase = bp.HourPriceBase,
                            RegistrationFeeFirstSemester = bp.RegistrationFeeFirstSemester,
                            Capacity = bp.Capacity,
                            IsActive = bp.IsActive,
                            IsApprovedByBtecAuthority = bp.IsApprovedByBtecAuthority
                        }).ToList(),

                    // Representatives
                    Representatives = university.Representatives
                        .Where(r => r.IsActive)
                        .OrderBy(r => r.User.FullName)
                        .Select(r => new RepresentativeInfoDto
                        {
                            FullName = r.User?.FullName ?? "Unknown",
                            Email = r.User?.Email ?? "N/A",
                            Position = r.Position,
                            AssignedAt = r.AssignedAt
                        }).ToList(),

                    // Current user permissions
                    CanManagePrograms = rep.CanManagePrograms,
                    CanManageFees = rep.CanManageFees,
                    CanViewApplications = rep.CanViewApplications
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading university details for representative");
                TempData["Error"] = "An error occurred while loading university details.";
                return RedirectToAction("Dashboard", "UniversityRep");
            }
        }
        #endregion

        #region Edit University
        // GET: /UniversityRep/University/Edit
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var rep = await _context.UniversityRepresentatives
                    .Include(r => r.University)
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

                if (rep == null)
                {
                    _logger.LogWarning($"No active university representative found for user {userId}");
                    TempData["Error"] = "You are not assigned to any university.";
                    return RedirectToAction("Dashboard", "UniversityRep");
                }

                // Check if representative has permission to manage university info
                // Note: We allow basic info editing for all reps, but fees require CanManageFees

                var university = rep.University;

                var viewModel = new EditUniversityViewModel
                {
                    Id = university.Id,
                    NameEnglish = university.NameEnglish,
                    NameArabic = university.NameArabic,
                    Province = university.Province,
                    City = university.City,
                    FullAddress = university.FullAddress,
                    Latitude = university.Latitude,
                    Longitude = university.Longitude,
                    AcademicAccreditation = university.AcademicAccreditation,
                    PhoneNumber = university.PhoneNumber,
                    Email = university.Email,
                    OfficialWebsite = university.OfficialWebsite,
                    CurrentLogoPath = university.LogoPath,
                    CurrentImagePath = university.ImagePath,

                    // Permissions
                    CanEditBasicInfo = true, // All reps can edit basic info
                    CanUploadImages = true   // All reps can upload images
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for university");
                TempData["Error"] = "An error occurred while loading the edit form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /UniversityRep/University/Edit
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUniversityViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = _userManager.GetUserId(User);
                var rep = await _context.UniversityRepresentatives
                    .Include(r => r.University)
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

                if (rep == null)
                {
                    TempData["Error"] = "You are not assigned to any university.";
                    return RedirectToAction("Dashboard", "UniversityRep");
                }

                if (rep.UniversityId != model.Id)
                {
                    _logger.LogWarning($"User {userId} attempted to edit university {model.Id} but is assigned to {rep.UniversityId}");
                    TempData["Error"] = "You can only edit your assigned university.";
                    return RedirectToAction(nameof(Index));
                }

                var university = rep.University;

                // Update only allowed fields (UniversityRep cannot change Type, CommissionMode, IsActive)
                university.NameEnglish = model.NameEnglish;
                university.NameArabic = model.NameArabic;
                university.Province = model.Province;
                university.City = model.City;
                university.FullAddress = model.FullAddress;
                university.Latitude = model.Latitude;
                university.Longitude = model.Longitude;
                university.AcademicAccreditation = model.AcademicAccreditation;
                university.PhoneNumber = model.PhoneNumber;
                university.Email = model.Email;
                university.OfficialWebsite = model.OfficialWebsite;
                university.UpdatedAt = DateTime.UtcNow;

                // Handle logo upload
                if (model.Logo != null && model.Logo.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(model.Logo.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("Logo", "Only image files (jpg, jpeg, png, gif) are allowed.");
                        return View(model);
                    }

                    // Validate file size (max 5MB)
                    if (model.Logo.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Logo", "Logo file size must be less than 5MB.");
                        return View(model);
                    }

                    // Delete old logo if exists
                    if (!string.IsNullOrEmpty(university.LogoPath))
                    {
                        DeleteFile(university.LogoPath);
                    }

                    university.LogoPath = await SaveFileAsync(model.Logo, "logos");
                }

                // Handle cover image upload
                if (model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(model.CoverImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("CoverImage", "Only image files (jpg, jpeg, png, gif) are allowed.");
                        return View(model);
                    }

                    // Validate file size (max 10MB)
                    if (model.CoverImage.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("CoverImage", "Cover image file size must be less than 10MB.");
                        return View(model);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(university.ImagePath))
                    {
                        DeleteFile(university.ImagePath);
                    }

                    university.ImagePath = await SaveFileAsync(model.CoverImage, "images");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"University updated: {university.NameEnglish} (ID: {university.Id}) by representative {User.Identity?.Name}");

                TempData["Success"] = "University information has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating university information");
                ModelState.AddModelError("", "An error occurred while updating university information.");
                return View(model);
            }
        }
        #endregion

        #region Helper Methods
        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var relativePath = Path.Combine("uploads", "universities", subFolder, fileName);
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{relativePath.Replace("\\", "/")}";
        }

        private void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation($"Deleted file: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
            }
        }
        #endregion
    }
}
