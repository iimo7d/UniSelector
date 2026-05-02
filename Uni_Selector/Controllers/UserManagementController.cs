using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.AdminCommission;
using Uni_Selector.ViewModels.UserManagement;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.PlatformAdmin)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? role = null,
            bool? isActive = null)
        {
            try
            {
                var usersQuery = _userManager.Users.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    usersQuery = usersQuery.Where(u =>
                        u.FullName.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm) ||
                        u.PhoneNumber.Contains(searchTerm));
                }

                // Apply active filter
                if (isActive.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.IsActive == isActive.Value);
                }

                var allUsers = await usersQuery.ToListAsync();

                // Filter by role
                if (!string.IsNullOrWhiteSpace(role))
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
                    allUsers = allUsers.Where(u => userIdsInRole.Contains(u.Id)).ToList();
                }

                var totalUsers = allUsers.Count;

                // Get paginated users
                var users = allUsers
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Build view models with roles
                var userViewModels = new List<UserListItemViewModel>();
                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    userViewModels.Add(new UserListItemViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber ?? "N/A",
                        IsActive = user.IsActive,
                        EmailConfirmed = user.EmailConfirmed,
                        LockoutEnabled = user.LockoutEnabled,
                        LockoutEnd = user.LockoutEnd,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        Roles = userRoles.ToList()
                    });
                }

                // Calculate summary statistics
                var activeUsers = allUsers.Count(u => u.IsActive);
                var inactiveUsers = allUsers.Count(u => !u.IsActive);

                // Get role counts
                var roleCounts = new Dictionary<string, int>();
                var allRoles = new[] { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                foreach (var roleName in allRoles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                    roleCounts[roleName] = usersInRole.Count;
                }

                var viewModel = new UserListViewModel
                {
                    Users = userViewModels,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalUsers = totalUsers,
                    SearchTerm = searchTerm,
                    Role = role,
                    IsActive = isActive,
                    TotalActiveUsers = activeUsers,
                    TotalInactiveUsers = inactiveUsers,
                    RoleCounts = roleCounts,
                    AllRoles = allRoles.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                TempData["Error"] = "An error occurred while loading users.";
                return View(new UserListViewModel());
            }
        }






        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "No Role";

                var viewModel = new UserDetailsViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "N/A",
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roles.ToList()
                };

                // Load role-specific data and statistics
                if (primaryRole == "Student")
                {
                    var student = await _context.Students
                        .Include(s => s.Applications)
                        .FirstOrDefaultAsync(s => s.UserId == user.Id);

                    if (student != null)
                    {
                        viewModel.StudentDetails = new StudentDetailsData
                        {
                            StudentId = student.Id,
                            DateOfBirth = student.DateOfBirth,
                            Gender = student.Gender,
                            GPA = student.GPA,
                            Province = student.Province,
                            City = student.City,
                            Path = student.Path,
                            RegistrationBudget = student.RegistrationBudget,
                            ProfileCompleted = student.ProfileCompleted ?? false,
                            TotalApplications = student.Applications.Count,
                            PendingApplications = student.Applications.Count(a => a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.UnderReview),
                            ApprovedApplications = student.Applications.Count(a => a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Enrolled),
                            RejectedApplications = student.Applications.Count(a => a.Status == ApplicationStatus.Rejected)
                        };

                        viewModel.ApplicationsCount = student.Applications.Count;
                    }
                }
                else if (primaryRole == "UniversityRepresentative")
                {
                    var rep = await _context.UniversityRepresentatives
                        .Include(r => r.University)
                            .ThenInclude(u => u.UniversityPrograms)
                        .Include(r => r.University)
                            .ThenInclude(u => u.BtecPrograms)
                        .FirstOrDefaultAsync(r => r.UserId == user.Id);

                    if (rep != null)
                    {
                        var pendingApplicationsCount = await _context.StudentApplications
                            .Where(a => (a.UniversityProgram.UniversityId == rep.UniversityId ||
                                        a.BtecProgram.UniversityId == rep.UniversityId) &&
                                       (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.UnderReview))
                            .CountAsync();

                        viewModel.UniversityRepDetails = new UniversityRepDetailsData
                        {
                            RepresentativeId = rep.Id,
                            UniversityId = rep.UniversityId,
                            UniversityName = rep.University.NameEnglish,
                            Position = rep.Position ?? "N/A",
                            UniversityProgramsCount = rep.University.UniversityPrograms.Count + rep.University.BtecPrograms.Count,
                            PendingApplicationsCount = pendingApplicationsCount
                        };
                    }
                }

                // Get notification count
                viewModel.NotificationsCount = await _context.Notifications
                    .Where(n => n.UserId == user.Id)
                    .CountAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading details for user {id}");
                TempData["Error"] = "An error occurred while loading user details.";
                return RedirectToAction(nameof(Index));
            }
        }




        public async Task<IActionResult> Create()
        {
            try
            {
                var universities = await _context.Universities
                    .Where(u => u.IsActive)
                    .Select(u => new UniversityOption
                    {
                        Id = u.Id,
                        Name = u.NameEnglish
                    })
                    .OrderBy(u => u.Name)
                    .ToListAsync();

                var viewModel = new UserCreateViewModel
                {
                    AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" },
                    AvailableUniversities = universities
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create user form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                    model.AvailableUniversities = await _context.Universities
                        .Where(u => u.IsActive)
                        .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                        .OrderBy(u => u.Name)
                        .ToListAsync();
                    return View(model);
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                    model.AvailableUniversities = await _context.Universities
                        .Where(u => u.IsActive)
                        .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                        .OrderBy(u => u.Name)
                        .ToListAsync();
                    return View(model);
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = model.IsActive,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                    model.AvailableUniversities = await _context.Universities
                        .Where(u => u.IsActive)
                        .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                        .OrderBy(u => u.Name)
                        .ToListAsync();
                    return View(model);
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, model.Role);

                // Create role-specific records
                if (model.Role == "Student")
                {
                    var student = new Student
                    {
                        UserId = user.Id,
                        DateOfBirth = model.DateOfBirth ?? DateTime.Now.AddYears(-18),
                        Gender = model.Gender ?? Gender.Male,
                        GPA = model.GPA ?? 0,
                        Province = model.Province ?? "Amman",
                        City = model.City ?? "Amman",
                        Path = model.Path ?? PathType.Academic,
                        AcademicTrack = model.AcademicTrack,
                        VocationalBranch = model.VocationalBranch,
                        RegistrationBudget = model.RegistrationBudget ?? 0,
                        ProfileCompleted = false,
                        NationalId = "",
                        SeatNumber = "",
                        Nationality = "Jordanian",
                        GuardianName = "",
                        GuardianPhone = "",
                        GuardianRelation = "",
                        IsActive = true
                    };
                    _context.Students.Add(student);
                }
                else if (model.Role == "UniversityRepresentative")
                {
                    if (!model.UniversityId.HasValue)
                    {
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError("UniversityId", "University is required for University Representatives.");
                        model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                        model.AvailableUniversities = await _context.Universities
                            .Where(u => u.IsActive)
                            .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                            .OrderBy(u => u.Name)
                            .ToListAsync();
                        return View(model);
                    }

                    var rep = new UniversityRepresentative
                    {
                        UserId = user.Id,
                        UniversityId = model.UniversityId.Value,
                        Position = model.Position,
                        CanManagePrograms = true,
                        CanManageFees = true,
                        CanViewApplications = true,
                        IsActive = true,
                        AssignedAt = DateTime.UtcNow
                    };
                    _context.UniversityRepresentatives.Add(rep);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {user.Email} created successfully with role {model.Role}");
                TempData["Success"] = $"User {user.FullName} created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "An error occurred while creating the user.");
                model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                model.AvailableUniversities = await _context.Universities
                    .Where(u => u.IsActive)
                    .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "Student";

                var viewModel = new UserEditViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = primaryRole,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" },
                    AvailableUniversities = await _context.Universities
                        .Where(u => u.IsActive)
                        .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                        .OrderBy(u => u.Name)
                        .ToListAsync()
                };

                // Load role-specific data
                if (primaryRole == "Student")
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    if (student != null)
                    {
                        viewModel.StudentId = student.Id;
                        viewModel.DateOfBirth = student.DateOfBirth;
                        viewModel.Gender = student.Gender;
                        viewModel.GPA = student.GPA;
                        viewModel.Province = student.Province;
                        viewModel.City = student.City;
                        viewModel.Path = student.Path;
                        viewModel.AcademicTrack = student.AcademicTrack;
                        viewModel.VocationalBranch = student.VocationalBranch;
                        viewModel.RegistrationBudget = student.RegistrationBudget;
                        viewModel.ProfileCompleted = student.ProfileCompleted;
                    }
                }
                else if (primaryRole == "UniversityRepresentative")
                {
                    var rep = await _context.UniversityRepresentatives.FirstOrDefaultAsync(r => r.UserId == user.Id);
                    if (rep != null)
                    {
                        viewModel.UniversityRepresentativeId = rep.Id;
                        viewModel.UniversityId = rep.UniversityId;
                        viewModel.Position = rep.Position;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for user {id}");
                TempData["Error"] = "An error occurred while loading the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["Error"] = "Invalid user ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                    model.AvailableUniversities = await _context.Universities
                        .Where(u => u.IsActive)
                        .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                        .OrderBy(u => u.Name)
                        .ToListAsync();
                    return View(model);
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if email is being changed
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email is already in use.");
                        model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                        model.AvailableUniversities = await _context.Universities
                            .Where(u => u.IsActive)
                            .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                            .OrderBy(u => u.Name)
                            .ToListAsync();
                        return View(model);
                    }
                }

                // Update basic user info
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.IsActive = model.IsActive;
                user.EmailConfirmed = model.EmailConfirmed;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                    model.AvailableUniversities = await _context.Universities
                        .Where(u => u.IsActive)
                        .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                        .OrderBy(u => u.Name)
                        .ToListAsync();
                    return View(model);
                }

                // Update role if changed
                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRole = currentRoles.FirstOrDefault();

                if (currentRole != model.Role)
                {
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await _userManager.RemoveFromRoleAsync(user, currentRole);
                    }
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                // Update role-specific data
                if (model.Role == "Student")
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    if (student == null)
                    {
                        student = new Student
                        {
                            UserId = user.Id,
                            DateOfBirth = model.DateOfBirth ?? DateTime.Now.AddYears(-18),
                            Gender = model.Gender ?? Gender.Male,
                            GPA = model.GPA ?? 0,
                            Province = model.Province ?? "Amman",
                            City = model.City ?? "Amman",
                            Path = model.Path ?? PathType.Academic,
                            AcademicTrack = model.AcademicTrack,
                            VocationalBranch = model.VocationalBranch,
                            RegistrationBudget = model.RegistrationBudget ?? 0,
                            ProfileCompleted = model.ProfileCompleted ?? false,
                            NationalId = "",
                            SeatNumber = "",
                            Nationality = "Jordanian",
                            GuardianName = "",
                            GuardianPhone = "",
                            GuardianRelation = "",
                            IsActive = true
                        };
                        _context.Students.Add(student);
                    }
                    else
                    {
                        student.DateOfBirth = model.DateOfBirth ?? student.DateOfBirth;
                        student.Gender = model.Gender ?? student.Gender;
                        student.GPA = model.GPA ?? student.GPA;
                        student.Province = model.Province ?? student.Province;
                        student.City = model.City ?? student.City;
                        student.Path = model.Path ?? student.Path;
                        student.AcademicTrack = model.AcademicTrack ?? student.AcademicTrack;
                        student.VocationalBranch = model.VocationalBranch ?? student.VocationalBranch;
                        student.RegistrationBudget = model.RegistrationBudget ?? student.RegistrationBudget;
                        student.ProfileCompleted = model.ProfileCompleted ?? student.ProfileCompleted;
                    }
                }
                else if (model.Role == "UniversityRepresentative")
                {
                    if (!model.UniversityId.HasValue)
                    {
                        ModelState.AddModelError("UniversityId", "University is required for University Representatives.");
                        model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                        model.AvailableUniversities = await _context.Universities
                            .Where(u => u.IsActive)
                            .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                            .OrderBy(u => u.Name)
                            .ToListAsync();
                        return View(model);
                    }

                    var rep = await _context.UniversityRepresentatives.FirstOrDefaultAsync(r => r.UserId == user.Id);
                    if (rep == null)
                    {
                        rep = new UniversityRepresentative
                        {
                            UserId = user.Id,
                            UniversityId = model.UniversityId.Value,
                            Position = model.Position,
                            CanManagePrograms = true,
                            CanManageFees = true,
                            CanViewApplications = true,
                            IsActive = true,
                            AssignedAt = DateTime.UtcNow
                        };
                        _context.UniversityRepresentatives.Add(rep);
                    }
                    else
                    {
                        rep.UniversityId = model.UniversityId.Value;
                        rep.Position = model.Position;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {user.Email} updated successfully");
                TempData["Success"] = $"User {user.FullName} updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {id}");
                ModelState.AddModelError("", "An error occurred while updating the user.");
                model.AvailableRoles = new List<string> { "Student", "UniversityRepresentative", "PlatformAdmin", "BtecAuthority" };
                model.AvailableUniversities = await _context.Universities
                    .Where(u => u.IsActive)
                    .Select(u => new UniversityOption { Id = u.Id, Name = u.NameEnglish })
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                return View(model);
            }
        }

        [HttpPost("{id}/Lock")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

                if (result.Succeeded)
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    _logger.LogInformation($"User {user.Email} locked by admin");
                    TempData["Success"] = $"User {user.FullName} has been locked.";
                }
                else
                {
                    TempData["Error"] = "Failed to lock user account.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error locking user {id}");
                TempData["Error"] = "An error occurred while locking the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("{id}/Unlock")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, null);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.Email} unlocked by admin");
                    TempData["Success"] = $"User {user.FullName} has been unlocked.";
                }
                else
                {
                    TempData["Error"] = "Failed to unlock user account.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unlocking user {id}");
                TempData["Error"] = "An error occurred while unlocking the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("{id}/ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                _logger.LogInformation($"Password reset requested for user {user.Email}. Token: {token}");

                TempData["Success"] = $"Password reset email sent to {user.Email}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for user {id}");
                TempData["Error"] = "An error occurred while resetting the password.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export()
        {
            try
            {
                var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Users");

                // Headers
                worksheet.Cell(1, 1).Value = "Full Name";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "Phone";
                worksheet.Cell(1, 4).Value = "Role";
                worksheet.Cell(1, 5).Value = "Active";
                worksheet.Cell(1, 6).Value = "Email Confirmed";
                worksheet.Cell(1, 7).Value = "Locked";
                worksheet.Cell(1, 8).Value = "Created Date";
                worksheet.Cell(1, 9).Value = "Last Login";

                var headerRange = worksheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                // Data
                int row = 2;
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "No Role";
                    var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

                    worksheet.Cell(row, 1).Value = user.FullName;
                    worksheet.Cell(row, 2).Value = user.Email;
                    worksheet.Cell(row, 3).Value = user.PhoneNumber ?? "N/A";
                    worksheet.Cell(row, 4).Value = primaryRole;
                    worksheet.Cell(row, 5).Value = user.IsActive ? "Yes" : "No";
                    worksheet.Cell(row, 6).Value = user.EmailConfirmed ? "Yes" : "No";
                    worksheet.Cell(row, 7).Value = isLocked ? "Yes" : "No";
                    worksheet.Cell(row, 8).Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 9).Value = user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = $"Users_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users");
                TempData["Error"] = "An error occurred while exporting users.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
