using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "BtecAuthority")]
    [Route("BTECAuthority/Standards")]
    public class BTECStandardsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BTECStandardsController> _logger;
        private readonly IBtecStandardsNotifier _btecStandardsNotifier;

        public BTECStandardsController(AppDbContext context,
            UserManager<ApplicationUser> userManager, 
            ILogger<BTECStandardsController> logger,
            IBtecStandardsNotifier btecStandardsNotifier)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _btecStandardsNotifier = btecStandardsNotifier;
        }



        #region View Standards

        /// <summary>
        /// View current BTEC standards - GET /BTECAuthority/Standards
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new BTECStandardsViewModel();

                // Standards by Level
                var standards = new List<StandardByLevelDto>
                {
                    new StandardByLevelDto
                    {
                        Level = BtecLevel.Level2,
                        MinimumGPA = 60,
                        MinimumCreditHours = 60,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = false,
                        Description = "Foundation level technical education suitable for secondary school graduates"
                    },
                    new StandardByLevelDto
                    {
                        Level = BtecLevel.Level3,
                        MinimumGPA = 65,
                        MinimumCreditHours = 90,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = true,
                        Description = "Advanced level technical education, equivalent to A-levels"
                    },
                    new StandardByLevelDto
                    {
                        Level = BtecLevel.Level4,
                        MinimumGPA = 70,
                        MinimumCreditHours = 120,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = true,
                        Description = "Higher education certificate level, first year of university"
                    },
                    new StandardByLevelDto
                    {
                        Level = BtecLevel.Level5,
                        MinimumGPA = 70,
                        MinimumCreditHours = 120,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = true,
                        Description = "Higher education diploma level, equivalent to second year of university"
                    },
                    new StandardByLevelDto
                    {
                        Level = BtecLevel.Level6,
                        MinimumGPA = 75,
                        MinimumCreditHours = 120,
                        RecommendedDuration = 1,
                        RequiresPreviousLevel = true,
                        Description = "Bachelor's degree level qualification"
                    },
                    new StandardByLevelDto
                    {
                        Level = BtecLevel.Level7,
                        MinimumGPA = 80,
                        MinimumCreditHours = 180,
                        RecommendedDuration = 1,
                        RequiresPreviousLevel = true,
                        Description = "Master's degree level qualification"
                    },
                    new StandardByLevelDto
                    {
                        Level = BtecLevel.Level8,
                        MinimumGPA = 85,
                        MinimumCreditHours = 240,
                        RecommendedDuration = 3,
                        RequiresPreviousLevel = true,
                        Description = "Doctoral level qualification"
                    }
                };

                model.Standards = standards;
                model.StandardsByLevel = standards;

                // Quality Requirements
                model.QualityRequirements = new List<QualityRequirementDto>
                {
                    new QualityRequirementDto { Name = "Qualified Instructors", Title = "Qualified Instructors", Description = "All instructors must hold relevant qualifications", IsMandatory = true, Category = "Faculty" },
                    new QualityRequirementDto { Name = "Modern Facilities", Title = "Modern Facilities", Description = "Up-to-date equipment and facilities", IsMandatory = true, Category = "Infrastructure" },
                    new QualityRequirementDto { Name = "Industry Partnership", Title = "Industry Partnership", Description = "Active partnerships with industry", IsMandatory = false, Category = "Partnerships" },
                    new QualityRequirementDto { Name = "Minimum Pass Rate", Title = "Minimum Pass Rate", Description = "At least 75% student pass rate", IsMandatory = true, Category = "Performance" },
                    new QualityRequirementDto { Name = "Annual Review", Title = "Annual Review", Description = "Program must undergo annual quality review", IsMandatory = true, Category = "Quality Assurance" }
                };

                // Accreditation Criteria
                model.AccreditationCriteria = new List<AccreditationCriteriaDto>
                {
                    new AccreditationCriteriaDto { Name = "Curriculum Quality", Criterion = "Curriculum Quality", Description = "Comprehensive and up-to-date curriculum", Weight = 25, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Faculty Qualifications", Criterion = "Faculty Qualifications", Description = "Qualified and experienced instructors", Weight = 20, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Facilities & Equipment", Criterion = "Facilities & Equipment", Description = "Modern facilities and equipment", Weight = 15, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Student Support", Criterion = "Student Support", Description = "Adequate student support services", Weight = 15, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Industry Links", Criterion = "Industry Links", Description = "Industry partnerships and placements", Weight = 15, IsRequired = false },
                    new AccreditationCriteriaDto { Name = "Research & Innovation", Criterion = "Research & Innovation", Description = "Research activities and innovation", Weight = 10, IsRequired = false }
                };

                model.LastUpdated = DateTime.UtcNow.AddMonths(-2); // Placeholder
                model.LastUpdatedBy = "BTEC Authority";

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading BTEC standards");
                TempData["Error"] = "Failed to load BTEC standards";
                return RedirectToAction("Dashboard", "BTECAuthority");
            }
        }

        #endregion

        #region Manage Standards

        /// <summary>
        /// Manage BTEC standards page - GET /BTECAuthority/Standards/Manage
        /// </summary>
        [HttpGet("Manage")]
        public async Task<IActionResult> Manage()
        {
            try
            {
                var model = new ManageBTECStandardsViewModel();

                // Level Standards
                model.LevelStandards = new List<LevelStandardDto>
                {
                    new LevelStandardDto
                    {
                        Level = BtecLevel.Level2,
                        MinimumGPA = 60,
                        MinimumCreditHours = 60,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = false,
                        Description = "Foundation level technical education",
                        KeyLearningOutcomes = new List<string> { "Basic technical skills", "Foundational knowledge", "Workplace readiness" }
                    },
                    new LevelStandardDto
                    {
                        Level = BtecLevel.Level3,
                        MinimumGPA = 65,
                        MinimumCreditHours = 90,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = true,
                        Description = "Advanced level technical education",
                        KeyLearningOutcomes = new List<string> { "Advanced technical skills", "Problem-solving", "Critical thinking" }
                    },
                    new LevelStandardDto
                    {
                        Level = BtecLevel.Level4,
                        MinimumGPA = 70,
                        MinimumCreditHours = 120,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = true,
                        Description = "Higher education certificate level",
                        KeyLearningOutcomes = new List<string> { "Specialized knowledge", "Independent learning", "Research skills" }
                    },
                    new LevelStandardDto
                    {
                        Level = BtecLevel.Level5,
                        MinimumGPA = 70,
                        MinimumCreditHours = 120,
                        RecommendedDuration = 2,
                        RequiresPreviousLevel = true,
                        Description = "Higher education diploma level",
                        KeyLearningOutcomes = new List<string> { "Expert knowledge", "Advanced research", "Leadership skills" }
                    },
                    new LevelStandardDto
                    {
                        Level = BtecLevel.Level6,
                        MinimumGPA = 75,
                        MinimumCreditHours = 120,
                        RecommendedDuration = 1,
                        RequiresPreviousLevel = true,
                        Description = "Bachelor's degree level",
                        KeyLearningOutcomes = new List<string> { "Professional competence", "Strategic thinking", "Innovation" }
                    },
                    new LevelStandardDto
                    {
                        Level = BtecLevel.Level7,
                        MinimumGPA = 80,
                        MinimumCreditHours = 180,
                        RecommendedDuration = 1,
                        RequiresPreviousLevel = true,
                        Description = "Master's degree level",
                        KeyLearningOutcomes = new List<string> { "Advanced professional expertise", "Research leadership", "Strategic planning" }
                    },
                    new LevelStandardDto
                    {
                        Level = BtecLevel.Level8,
                        MinimumGPA = 85,
                        MinimumCreditHours = 240,
                        RecommendedDuration = 3,
                        RequiresPreviousLevel = true,
                        Description = "Doctoral level",
                        KeyLearningOutcomes = new List<string> { "Original research", "Industry leadership", "Academic excellence" }
                    }
                };

                // General Requirements (flat properties)
                model.MinimumGPA = 60;
                model.MinimumCreditHours = 60;
                model.MaximumDuration = 5;
                model.RequireEnglishProficiency = true;
                model.RequireTechnicalBackground = false;

                // Quality Standards (flat properties)
                model.RequireQualifiedInstructors = true;
                model.RequireModernFacilities = true;
                model.RequireIndustryPartnership = false;
                model.MinimumPassRate = 75;
                model.RequireAnnualReview = true;

                // Accreditation Criteria
                model.AccreditationCriteria = new List<AccreditationCriteriaDto>
                {
                    new AccreditationCriteriaDto { Name = "Curriculum Quality", Criterion = "Curriculum Quality", Description = "Comprehensive curriculum", Weight = 25, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Faculty Qualifications", Criterion = "Faculty Qualifications", Description = "Qualified instructors", Weight = 20, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Facilities", Criterion = "Facilities", Description = "Modern facilities", Weight = 15, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Student Support", Criterion = "Student Support", Description = "Support services", Weight = 15, IsRequired = true },
                    new AccreditationCriteriaDto { Name = "Industry Links", Criterion = "Industry Links", Description = "Industry partnerships", Weight = 15, IsRequired = false },
                    new AccreditationCriteriaDto { Name = "Research", Criterion = "Research", Description = "Research activities", Weight = 10, IsRequired = false }
                };

                // Notification settings
                model.NotifyUniversities = true;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manage standards page");
                TempData["Error"] = "Failed to load standards management page";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Update Standards (with Notifications & Emails)

        /// <summary>
        /// Update BTEC standards - POST /BTECAuthority/Standards/Update
        /// </summary>
        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateBTECStandardsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please provide all required information";
                    return RedirectToAction("Manage");
                }

                // Get current user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Manage");
                }

                // WARNING: This action only logs the standards announcement and sends notifications.
                // The standards values are NOT persisted to the database because there is no BtecStandards
                // DB table/entity yet. To add persistence: create a BtecStandardsConfig entity, add it to
                // ApplicationDbContext, and call _context.SaveChangesAsync() here.
                _logger.LogInformation("BTEC Standards notice published by {User} at {Time} (NOT persisted to DB)",
                    currentUser.FullName, DateTime.UtcNow);
                _logger.LogInformation("Update Description: {Description}", model.UpdateDescription);
                _logger.LogInformation("Effective Date: {Date}", model.EffectiveDate);

                // If NotifyUniversities is true, send notifications and emails
                if (model.NotifyUniversities)
                {
                    await _btecStandardsNotifier.SendStandardsUpdateNotificationsAsync(model, currentUser);
                }

                TempData["Success"] = "BTEC standards notice has been published" +
                    (model.NotifyUniversities ? " and university representatives have been notified." : ".") +
                    " Note: values are not yet persisted to the database.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating BTEC standards");
                TempData["Error"] = "Failed to update BTEC standards. Please try again.";
                return RedirectToAction("Manage");
            }
        }

        #endregion

        

    }
}