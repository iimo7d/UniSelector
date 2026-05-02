using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Hubs;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.UniversityAdmin;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.PlatformAdmin)]
    public class UniversityAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<UniversityAdminController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UniversityAdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> hubContext,
            ILogger<UniversityAdminController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Index & List
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? province, bool? isActive, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Universities
                    .Include(u => u.Representatives)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(u =>
                        u.NameEnglish.Contains(searchTerm) ||
                        u.NameArabic.Contains(searchTerm) ||
                        u.City.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(province))
                {
                    query = query.Where(u => u.Province == province);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                var totalUniversities = await query.CountAsync();

                var universities = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UniversityListItemDto
                    {
                        Id = u.Id,
                        NameEnglish = u.NameEnglish,
                        NameArabic = u.NameArabic,
                        LogoPath = u.LogoPath,
                        City = u.City,
                        Province = u.Province,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Type = u.Type.ToString(),
                        IsActive = u.IsActive,
                        RepresentativesCount = u.Representatives.Count,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                var provinces = await _context.Universities
                    .Select(u => u.Province)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync();

                var viewModel = new UniversityListViewModel
                {
                    Universities = universities,
                    SearchTerm = searchTerm,
                    Province = province,
                    IsActive = isActive,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalUniversities / (double)pageSize),
                    TotalUniversities = totalUniversities,
                    Provinces = provinces
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading universities list");
                TempData["Error"] = "An error occurred while loading universities.";
                return View(new UniversityListViewModel());
            }
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var university = await _context.Universities
                    .Include(u => u.Representatives)
                        .ThenInclude(r => r.User)
                    .Include(u => u.UniversityPrograms)
                        .ThenInclude(up => up.Program)
                    .Include(u => u.BtecPrograms)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                var applicationStats = await _context.StudentApplications
                    .Where(sa => (sa.UniversityProgram != null && sa.UniversityProgram.UniversityId == id) ||
                                 (sa.BtecProgram != null && sa.BtecProgram.UniversityId == id))
                    .GroupBy(sa => sa.Status)
                    .Select(g => new ApplicationStatusDto
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
                    TotalPrograms = university.UniversityPrograms.Count + university.BtecPrograms.Count,
                    ActivePrograms = university.UniversityPrograms.Count(up => up.IsActive) +
                                    university.BtecPrograms.Count(bp => bp.IsActive),
                    TotalRepresentatives = university.Representatives.Count,
                    ActiveRepresentatives = university.Representatives.Count(r => r.IsActive),
                    UniversityPrograms = university.UniversityPrograms.Select(up => new UniversityProgramDto
                    {
                        Id = up.Id,
                        ProgramNameEnglish = up.Program.NameEnglish,
                        ProgramNameArabic = up.Program.NameArabic,
                        Degree = up.Program.Degree.ToString(),
                        DurationInYears = up.DurationInYears,
                        IsActive = up.IsActive
                    }).ToList(),
                    Representatives = university.Representatives.Select(r => new UniversityRepresentativeDto
                    {
                        Id = r.Id,
                        UserFullName = r.User.FullName,
                        UserEmail = r.User.Email,
                        Position = r.Position,
                        IsActive = r.IsActive,
                        AssignedAt = r.AssignedAt
                    }).ToList(),
                    ApplicationStats = applicationStats
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading details for university ID: {id}");
                TempData["Error"] = "An error occurred while loading university details.";
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                ViewBag.CommissionModes = new SelectList(Enum.GetValues(typeof(CommissionMode)));
                ViewBag.UniversityTypes = new SelectList(Enum.GetValues(typeof(UniversityType)));

                return View(new CreateUniversityViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create university form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUniversityViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.CommissionModes = new SelectList(Enum.GetValues(typeof(CommissionMode)));
                    ViewBag.UniversityTypes = new SelectList(Enum.GetValues(typeof(UniversityType)));
                    return View(model);
                }

                var university = new University
                {
                    NameEnglish = model.NameEnglish,
                    NameArabic = model.NameArabic,
                    Type = model.Type,
                    Province = model.Province,
                    City = model.City,
                    FullAddress = model.FullAddress,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    AcademicAccreditation = model.AcademicAccreditation,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    OfficialWebsite = model.OfficialWebsite,
                    CommissionMode = model.CommissionMode,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle logo upload
                if (model.Logo != null && model.Logo.Length > 0)
                {
                    university.LogoPath = await SaveFileAsync(model.Logo, "logos");
                }

                // Handle cover image upload
                if (model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    university.ImagePath = await SaveFileAsync(model.CoverImage, "images");
                }

                _context.Universities.Add(university);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"University created: {university.NameEnglish} (ID: {university.Id}) by admin {User.Identity?.Name}");

                TempData["Success"] = $"University '{university.NameEnglish}' has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating university");
                ModelState.AddModelError("", "An error occurred while creating the university.");
                ViewBag.CommissionModes = new SelectList(Enum.GetValues(typeof(CommissionMode)));
                ViewBag.UniversityTypes = new SelectList(Enum.GetValues(typeof(UniversityType)));
                return View(model);
            }
        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var university = await _context.Universities.FindAsync(id);

                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditUniversityViewModel
                {
                    Id = university.Id,
                    NameEnglish = university.NameEnglish,
                    NameArabic = university.NameArabic,
                    Type = university.Type,
                    Province = university.Province,
                    City = university.City,
                    FullAddress = university.FullAddress,
                    Latitude = university.Latitude,
                    Longitude = university.Longitude,
                    AcademicAccreditation = university.AcademicAccreditation,
                    PhoneNumber = university.PhoneNumber,
                    Email = university.Email,
                    OfficialWebsite = university.OfficialWebsite,
                    CommissionMode = university.CommissionMode,
                    CurrentLogoPath = university.LogoPath,
                    CurrentImagePath = university.ImagePath
                };

                ViewBag.CommissionModes = new SelectList(Enum.GetValues(typeof(CommissionMode)), viewModel.CommissionMode);
                ViewBag.UniversityTypes = new SelectList(Enum.GetValues(typeof(UniversityType)), viewModel.Type);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for university ID: {id}");
                TempData["Error"] = "An error occurred while loading the university details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUniversityViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["Error"] = "Invalid university ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.CommissionModes = new SelectList(Enum.GetValues(typeof(CommissionMode)), model.CommissionMode);
                    ViewBag.UniversityTypes = new SelectList(Enum.GetValues(typeof(UniversityType)), model.Type);
                    return View(model);
                }

                var university = await _context.Universities.FindAsync(id);
                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update properties
                university.NameEnglish = model.NameEnglish;
                university.NameArabic = model.NameArabic;
                university.Type = model.Type;
                university.Province = model.Province;
                university.City = model.City;
                university.FullAddress = model.FullAddress;
                university.Latitude = model.Latitude;
                university.Longitude = model.Longitude;
                university.AcademicAccreditation = model.AcademicAccreditation;
                university.PhoneNumber = model.PhoneNumber;
                university.Email = model.Email;
                university.OfficialWebsite = model.OfficialWebsite;
                university.CommissionMode = model.CommissionMode;
                university.UpdatedAt = DateTime.UtcNow;

                // Handle logo update
                if (model.Logo != null && model.Logo.Length > 0)
                {
                    // Delete old logo
                    if (!string.IsNullOrEmpty(university.LogoPath))
                    {
                        DeleteFile(university.LogoPath);
                    }
                    university.LogoPath = await SaveFileAsync(model.Logo, "logos");
                }

                // Handle cover image update
                if (model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    // Delete old image
                    if (!string.IsNullOrEmpty(university.ImagePath))
                    {
                        DeleteFile(university.ImagePath);
                    }
                    university.ImagePath = await SaveFileAsync(model.CoverImage, "images");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"University updated: {university.NameEnglish} (ID: {id}) by admin {User.Identity?.Name}");

                TempData["Success"] = $"University '{university.NameEnglish}' has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating university ID: {id}");
                ModelState.AddModelError("", "An error occurred while updating the university.");
                ViewBag.CommissionModes = new SelectList(Enum.GetValues(typeof(CommissionMode)), model.CommissionMode);
                ViewBag.UniversityTypes = new SelectList(Enum.GetValues(typeof(UniversityType)), model.Type);
                return View(model);
            }
        }
        #endregion

        #region Approve & Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var university = await _context.Universities
                    .Include(u => u.Representatives)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                university.IsActive = true;
                university.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send notifications to representatives
                await SendNotificationToRepresentatives(
                    university,
                    "University Approved",
                    $"The university '{university.NameEnglish}' has been approved by the platform administrator.",
                    "/UniversityRep/Dashboard"
                );

                _logger.LogInformation($"University approved: {university.NameEnglish} (ID: {id}) by admin {User.Identity?.Name}");

                TempData["Success"] = $"University '{university.NameEnglish}' has been approved successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving university ID: {id}");
                TempData["Error"] = "An error occurred while approving the university.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id, string? reason)
        {
            try
            {
                var university = await _context.Universities
                    .Include(u => u.Representatives)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                university.IsActive = false;
                university.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var message = string.IsNullOrWhiteSpace(reason)
                    ? $"The university '{university.NameEnglish}' has been suspended by the platform administrator."
                    : $"The university '{university.NameEnglish}' has been suspended. Reason: {reason}";

                await SendNotificationToRepresentatives(
                    university,
                    "University Suspended",
                    message,
                    "/UniversityRep/Dashboard"
                );

                _logger.LogInformation($"University suspended: {university.NameEnglish} (ID: {id}) by admin {User.Identity?.Name}. Reason: {reason ?? "Not specified"}");

                TempData["Success"] = $"University '{university.NameEnglish}' has been suspended successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error suspending university ID: {id}");
                TempData["Error"] = "An error occurred while suspending the university.";
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

        #region Representatives Management
        [HttpGet]
        public async Task<IActionResult> Representatives(int id)
        {
            try
            {
                var university = await _context.Universities
                    .Include(u => u.Representatives)
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                var existingRepUserIds = university.Representatives.Select(r => r.UserId).ToList();

                // Get all active users
                var allUsers = await _userManager.Users.Where(u => u.IsActive).ToListAsync();

                // Filter to only show students and normal users (exclude Admin, UniversityRep, BtecAuthority)
                var availableUsersList = new List<SelectListItem>();

                foreach (var user in allUsers)
                {
                    // Skip if already a representative
                    if (existingRepUserIds.Contains(user.Id))
                        continue;

                    // Get user roles
                    var roles = await _userManager.GetRolesAsync(user);

                    // Exclude users with Admin, UniversityRep, or BtecAuthority roles
                    if (!roles.Contains(UserRoles.PlatformAdmin) &&
                        !roles.Contains(UserRoles.UniversityRep) &&
                        !roles.Contains(UserRoles.BtecAuthority))
                    {
                        availableUsersList.Add(new SelectListItem
                        {
                            Value = user.Id,
                            Text = $"{user.FullName} ({user.Email})"
                        });
                    }
                }

                var viewModel = new UniversityRepresentativesViewModel
                {
                    University = new UniversityBasicInfoDto
                    {
                        Id = university.Id,
                        NameEnglish = university.NameEnglish,
                        NameArabic = university.NameArabic,
                        LogoPath = university.LogoPath,
                        IsActive = university.IsActive
                    },
                    Representatives = university.Representatives
                        .OrderByDescending(r => r.AssignedAt)
                        .Select(r => new RepresentativeDetailDto
                        {
                            Id = r.Id,
                            UserId = r.UserId,
                            UserFullName = r.User.FullName,
                            UserEmail = r.User.Email,
                            Position = r.Position,
                            CanManagePrograms = r.CanManagePrograms,
                            CanManageFees = r.CanManageFees,
                            CanViewApplications = r.CanViewApplications,
                            IsActive = r.IsActive,
                            AssignedAt = r.AssignedAt
                        }).ToList(),
                    AvailableUsers = availableUsersList
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading representatives for university ID: {id}");
                TempData["Error"] = "An error occurred while loading university representatives.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRepresentative(int id, string userId)
        {
            try
            {
                var university = await _context.Universities.FindAsync(id);
                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Representatives), new { id });
                }

                // Check if already assigned
                var existingRep = await _context.UniversityRepresentatives
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.UniversityId == id);

                if (existingRep != null)
                {
                    TempData["Error"] = "This user is already a representative of this university.";
                    return RedirectToAction(nameof(Representatives), new { id });
                }

                // Ensure user has UniversityRep role
                var isInRole = await _userManager.IsInRoleAsync(user, UserRoles.UniversityRep);
                if (!isInRole)
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.UniversityRep);
                }

                // Create representative
                var representative = new UniversityRepresentative
                {
                    UserId = userId,
                    UniversityId = id,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                    CanManagePrograms = true,
                    CanManageFees = true,
                    CanViewApplications = true
                };

                _context.UniversityRepresentatives.Add(representative);
                await _context.SaveChangesAsync();

                // Send notification
                await SendNotificationToUser(
                    userId,
                    "University Representative Assignment",
                    $"You have been assigned as a representative for '{university.NameEnglish}'.",
                    "/UniversityRep/Dashboard"
                );

                _logger.LogInformation($"Representative added: User {user.Email} to University {university.NameEnglish} (ID: {id}) by admin {User.Identity?.Name}");

                TempData["Success"] = $"{user.FullName} has been added as a representative for '{university.NameEnglish}'.";
                return RedirectToAction(nameof(Representatives), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding representative to university ID: {id}");
                TempData["Error"] = "An error occurred while adding the representative.";
                return RedirectToAction(nameof(Representatives), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRepresentative(CreateRepresentativeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please check all required fields.";
                    return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
                }

                var university = await _context.Universities.FindAsync(model.UniversityId);
                if (university == null)
                {
                    TempData["Error"] = "University not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    TempData["Error"] = "A user with this email already exists.";
                    return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
                }

                // Create new user
                var newUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // Auto-confirm for admin-created accounts
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["Error"] = $"Failed to create user: {errors}";
                    return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
                }

                // Assign UniversityRep role
                await _userManager.AddToRoleAsync(newUser, UserRoles.UniversityRep);

                // Create representative
                var representative = new UniversityRepresentative
                {
                    UserId = newUser.Id,
                    UniversityId = model.UniversityId,
                    Position = model.Position,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                    CanManagePrograms = model.CanManagePrograms,
                    CanManageFees = model.CanManageFees,
                    CanViewApplications = model.CanViewApplications
                };

                _context.UniversityRepresentatives.Add(representative);
                await _context.SaveChangesAsync();

                // Send notification
                await SendNotificationToUser(
                    newUser.Id,
                    "University Representative Account Created",
                    $"Your account has been created and you have been assigned as a representative for '{university.NameEnglish}'. Your login credentials have been set up.",
                    "/UniversityRep/Dashboard"
                );

                _logger.LogInformation($"Representative created: User {newUser.Email} for University {university.NameEnglish} (ID: {model.UniversityId}) by admin {User.Identity?.Name}");

                TempData["Success"] = $"Representative '{newUser.FullName}' has been created and assigned to '{university.NameEnglish}' successfully.";
                return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating representative for university ID: {model.UniversityId}");
                TempData["Error"] = "An error occurred while creating the representative.";
                return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRepresentative(int id, int repId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var representative = await _context.UniversityRepresentatives
                    .Include(r => r.User)
                    .Include(r => r.University)
                    .FirstOrDefaultAsync(r => r.Id == repId && r.UniversityId == id);

                if (representative == null)
                {
                    TempData["Error"] = "Representative not found.";
                    return RedirectToAction(nameof(Representatives), new { id });
                }

                var user = representative.User;
                var universityName = representative.University.NameEnglish;

                // 1️⃣ Remove representative record
                _context.UniversityRepresentatives.Remove(representative);
                await _context.SaveChangesAsync();

                // 2️⃣ Remove UniversityRep role
                if (await _userManager.IsInRoleAsync(user, UserRoles.UniversityRep))
                {
                    await _userManager.RemoveFromRoleAsync(user, UserRoles.UniversityRep);
                }

                // 3️⃣ Assign Student role (if not already)
                if (!await _userManager.IsInRoleAsync(user, UserRoles.Student))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Student);
                }

                // 4️⃣ Commit transaction
                await transaction.CommitAsync();

                // 5️⃣ Notify user
                await SendNotificationToUser(
                    user.Id,
                    "Representative Role Removed",
                    $"Your representative role for '{universityName}' has been removed. Your account has been updated to a student role.",
                    "/Account/Profile"
                );

                _logger.LogInformation(
                    $"User {user.Email} removed from UniversityRep and assigned Student role by admin {User.Identity?.Name}"
                );

                TempData["Success"] = $"{user.FullName} has been removed as a representative and reassigned as a student.";
                return RedirectToAction(nameof(Representatives), new { id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error removing representative ID {repId} from university ID {id}");
                TempData["Error"] = "An error occurred while removing the representative.";
                return RedirectToAction(nameof(Representatives), new { id });
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditRepresentative(int id, int repId)
        {
            try
            {
                var representative = await _context.UniversityRepresentatives
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == repId && r.UniversityId == id);

                if (representative == null)
                {
                    TempData["Error"] = "Representative not found.";
                    return RedirectToAction(nameof(Representatives), new { id });
                }

                var viewModel = new EditRepresentativeViewModel
                {
                    Id = representative.Id,
                    UniversityId = representative.UniversityId,
                    UserId = representative.UserId,
                    UserFullName = representative.User.FullName,
                    UserEmail = representative.User.Email,
                    Position = representative.Position,
                    CanManagePrograms = representative.CanManagePrograms,
                    CanManageFees = representative.CanManageFees,
                    CanViewApplications = representative.CanViewApplications,
                    IsActive = representative.IsActive
                };

                // Return JSON for AJAX request
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading representative ID: {repId} for editing");
                return Json(new { error = "An error occurred while loading the representative." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRepresentative(EditRepresentativeViewModel model)
        {
            try
            {
                if (model.Id <=0)
                {
                    TempData["Error"] = "Please fill in all required fields correctly.";
                    return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
                }

                var representative = await _context.UniversityRepresentatives
                    .Include(r => r.User)
                    .Include(r => r.University)
                    .FirstOrDefaultAsync(r => r.Id == model.Id && r.UniversityId == model.UniversityId);

                if (representative == null)
                {
                    TempData["Error"] = "Representative not found.";
                    return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
                }

                // Update representative details
                representative.Position = model.Position;
                representative.CanManagePrograms = model.CanManagePrograms;
                representative.CanManageFees = model.CanManageFees;
                representative.CanViewApplications = model.CanViewApplications;
                representative.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                // Send notification about permission changes
                await SendNotificationToUser(
                    representative.UserId,
                    "Representative Permissions Updated",
                    $"Your permissions as a representative for '{representative.University.NameEnglish}' have been updated by the platform administrator.",
                    "/UniversityRep/Dashboard"
                );

                _logger.LogInformation($"Representative updated: {representative.User.FullName} for University {representative.University.NameEnglish} (ID: {model.UniversityId}) by admin {User.Identity?.Name}");

                TempData["Success"] = $"Representative '{representative.User.FullName}' has been updated successfully.";
                return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating representative ID: {model.Id}");
                TempData["Error"] = "An error occurred while updating the representative.";
                return RedirectToAction(nameof(Representatives), new { id = model.UniversityId });
            }
        }
        #endregion

        #region Helper Methods
        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var relativePath = Path.Combine("uploads", "universities", subFolder, fileName);
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

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

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private async Task SendNotificationToRepresentatives(University university, string title, string message, string actionUrl)
        {
            foreach (var rep in university.Representatives.Where(r => r.IsActive))
            {
                await SendNotificationToUser(rep.UserId, title, message, actionUrl);
            }
        }

        private async Task SendNotificationToUser(string userId, string title, string message, string actionUrl)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Category = NotificationCategory.SystemAlert,
                Channel = NotificationChannel.InApp,
                ActionUrl = actionUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notification.Title, notification.Message, notification.ActionUrl);
        }
        #endregion
    }

}


