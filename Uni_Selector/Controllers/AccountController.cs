using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.Account;

namespace Uni_Selector.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger; 
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ILogger<AccountController> logger,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context
            )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

        // ENDPOINT 1: Login/Register Page (Unified)
        [HttpGet]
        public IActionResult Login(string returnUrl = null, string tab = "login")
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ActiveTab"] = tab.ToLower(); 

            return View();
        }

        // ENDPOINT 2: Handle Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return View(model);

            // Load user once (more efficient)
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.Now;
                await _userManager.UpdateAsync(user);      
                _logger.LogInformation($"User {user.Email} logged in.");

                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                if (lockoutEnd.HasValue)
                {
                    await _emailService.SendAccountLockedNotificationAsync(
                        user.Email,
                        user.FullName,
                        lockoutEnd.Value.LocalDateTime);
                }

                TempData["ErrorMessage"] =
                    "Account locked due to multiple failed login attempts. Check your email for details.";

                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // ENDPOINT 3: Handle Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Store errors in TempData so the Login view (LoginViewModel) can display them
                TempData["RegisterErrors"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["RegisterModel_Email"] = model.Email;
                TempData["RegisterModel_FullName"] = model.FullName;
                TempData["ShowRegisterTab"] = true;
                return RedirectToAction(nameof(Login));
            }


            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {

                if(!await _roleManager.RoleExistsAsync(UserRoles.Student))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Student));
                }

                // Assign default Student role
                await _userManager.AddToRoleAsync(user, UserRoles.Student);



                await _context.SaveChangesAsync();


                // Generate email confirmation token
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmUrl = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, token = emailToken }, Request.Scheme);

                // Send confirmation email
                await _emailService.SendEmailConfirmationAsync(user.Email, user.FullName, confirmUrl);

                // Auto sign-in
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = "Registration successful! Check your email to confirm your account.";
                return RedirectToAction("Index", "Home");
            }

            // Store registration errors in TempData and redirect back to Login (avoids model type mismatch)
            TempData["RegisterErrors"] = result.Errors.Select(e => e.Description).ToList();
            TempData["RegisterModel_Email"] = model.Email;
            TempData["RegisterModel_FullName"] = model.FullName;
            TempData["ShowRegisterTab"] = true;
            return RedirectToAction(nameof(Login));
        }

        // ENDPOINT 4: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // ENDPOINT 5: Forgot Password Page
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ENDPOINT 6: Handle Forgot Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action("ResetPassword", "Account",
                    new { token, email = user.Email }, Request.Scheme);

                await _emailService.SendPasswordResetAsync(user.Email, user.FullName, resetUrl);
            }

            // Always show success to prevent email enumeration
            TempData["SuccessMessage"] = "If that email exists, we've sent password reset instructions.";
            return RedirectToAction("ForgotPassword");
        }

        // ENDPOINT 7: Reset Password Page
        [HttpGet]
        public IActionResult ResetPassword(string token = null, string email = null)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid password reset token.");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // ENDPOINT 8: Handle Reset Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid password reset request.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);
                TempData["SuccessMessage"] = "Password reset successful! You can now login.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ENDPOINT 9: Email Confirmation
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

                TempData["SuccessMessage"] =
                    "Email confirmed successfully! Please complete your profile to continue.";

                return RedirectToAction(
                    actionName: "CompleteProfile",
                    controllerName: "Student");
            }

            TempData["ErrorMessage"] =
                "Email confirmation failed. The link may have expired.";

            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        public async Task<IActionResult> Me(string? id)
        {
            try
            {
                // If no id provided, use current logged-in user
                var userId = id ?? _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Get the user
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Check if the current user can view this profile
                var currentUserId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole(UserRoles.PlatformAdmin);

                if (currentUserId != userId && !isAdmin)
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this profile.";
                    return RedirectToAction("Index", "Home");
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "Unknown";

                // Build the view model
                var viewModel = new UserProfileViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Role = primaryRole
                };

                // Populate role-specific data
                if (primaryRole == UserRoles.Student)
                {
                    var student = await _context.Students
                        .Include(s => s.FamilyConnectionUniversity)
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (student != null)
                    {
                        viewModel.NationalId = student.NationalId;
                        viewModel.SeatNumber = student.SeatNumber;
                        viewModel.Gender = student.Gender.ToString();
                        viewModel.DateOfBirth = student.DateOfBirth;
                        viewModel.Nationality = student.Nationality;
                        viewModel.Province = student.Province;
                        viewModel.City = student.City;
                        viewModel.Area = student.Area;
                        viewModel.GPA = student.GPA;
                        viewModel.PathType = student.Path.ToString();
                        viewModel.AcademicTrack = student.AcademicTrack?.ToString();
                        viewModel.VocationalBranch = student.VocationalBranch?.ToString();
                        viewModel.BtecLevel2Completed = student.BtecLevel2Completed;
                        viewModel.BtecLevel3Completed = student.BtecLevel3Completed;
                        viewModel.BtecCertificateUrl = student.BtecCertificateUrl;
                        viewModel.RegistrationBudget = student.RegistrationBudget;
                        viewModel.DesiredMajors = student.DesiredMajors;
                        viewModel.PreferredCity = student.PreferredCity;
                        viewModel.MaxDistanceKm = student.MaxDistanceKm;
                        viewModel.PreferredLanguage = student.PreferredLanguage.ToString();
                        viewModel.HasFamilyConnection = student.HasFamilyConnection;
                        viewModel.FamilyConnectionUniversityName = student.FamilyConnectionUniversity?.NameEnglish;
                        viewModel.GuardianName = student.GuardianName;
                        viewModel.GuardianPhone = student.GuardianPhone;
                        viewModel.GuardianRelation = student.GuardianRelation;
                        viewModel.HasDisability = student.HasDisability;
                        viewModel.DisabilityType = student.DisabilityType;
                        viewModel.IsOrphan = student.IsOrphan;
                        viewModel.IsEmployeeChild = student.IsEmployeeChild;
                        viewModel.ProfileCompleted = student.ProfileCompleted;
                    }
                }
                else if (primaryRole == UserRoles.UniversityRep)
                {
                    var rep = await _context.UniversityRepresentatives
                        .Include(r => r.University)
                        .FirstOrDefaultAsync(r => r.UserId == userId);

                    if (rep != null)
                    {
                        viewModel.UniversityName = rep.University.NameEnglish;
                        viewModel.Position = rep.Position;
                        viewModel.CanManagePrograms = rep.CanManagePrograms;
                        viewModel.CanManageFees = rep.CanManageFees;
                        viewModel.CanViewApplications = rep.CanViewApplications;
                        viewModel.AssignedAt = rep.AssignedAt;
                        viewModel.RepIsActive = rep.IsActive;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the profile.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}


