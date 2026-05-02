using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.ViewModels.ProgramManagement;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = "UniversityRep")]
    [Route("UniversityRep/BTECPrograms")]
    public class BTECProgramManagementController : Controller
    {
        private readonly AppDbContext _context;

        public BTECProgramManagementController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /UniversityRep/BTECPrograms
        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, bool? isActive, bool? isApproved, int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null)
            {
                TempData["ErrorMessage"] = "You are not assigned to any university.";
                return RedirectToAction("Index", "UniversityRep");
            }

            // Check permissions
            if (!rep.CanManagePrograms)
            {
                TempData["ErrorMessage"] = "You do not have permission to manage BTEC programs.";
                return RedirectToAction("Index", "UniversityRep");
            }

            var query = _context.BtecPrograms
                .Include(bp => bp.University)
                .Where(bp => bp.UniversityId == rep.UniversityId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(bp =>
                    bp.NameArabic.Contains(search) ||
                    bp.NameEnglish.Contains(search) ||
                    bp.TechnicalField.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(bp => bp.IsActive == isActive.Value);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(bp => bp.IsApprovedByBtecAuthority == isApproved.Value);
            }

            var totalCount = await query.CountAsync();
            var programs = await query
                .OrderByDescending(bp => bp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bp => new BtecProgramListItemDto
                {
                    Id = bp.Id,
                    NameArabic = bp.NameArabic,
                    NameEnglish = bp.NameEnglish,
                    Level = bp.Level,
                    TechnicalField = bp.TechnicalField,
                    Language = bp.Language,
                    DurationInYears = bp.DurationInYears,
                    HourPriceBase = bp.HourPriceBase,
                    Capacity = bp.Capacity,
                    IsActive = bp.IsActive,
                    IsApprovedByBtecAuthority = bp.IsApprovedByBtecAuthority,
                    ApprovalDate = bp.ApprovalDate,
                    CreatedAt = bp.CreatedAt
                })
                .ToListAsync();

            var viewModel = new BtecProgramListViewModel
            {
                Programs = programs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Search = search,
                IsActiveFilter = isActive,
                IsApprovedFilter = isApproved,
                UniversityId = rep.UniversityId,
                CanManagePrograms = rep.CanManagePrograms
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/BTECPrograms/{id}/Details
        [HttpGet("{id}/Details")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null)
            {
                TempData["ErrorMessage"] = "You are not assigned to any university.";
                return RedirectToAction("Index", "UniversityRep");
            }

            var program = await _context.BtecPrograms
                .Include(bp => bp.University)
                .Include(bp => bp.EntryRequirements)
                .FirstOrDefaultAsync(bp => bp.Id == id && bp.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["ErrorMessage"] = "BTEC program not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new BtecProgramDetailsViewModel
            {
                Id = program.Id,
                NameArabic = program.NameArabic,
                NameEnglish = program.NameEnglish,
                Description = program.Description,
                Level = program.Level,
                TechnicalField = program.TechnicalField,
                Language = program.Language,
                DurationInYears = program.DurationInYears,
                SemesterStartDate = program.SemesterStartDate,
                TotalCreditHours = program.TotalCreditHours,
                HourPriceBase = program.HourPriceBase,
                RegistrationFeeFirstSemester = program.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = program.RegistrationFeeRegularSemester,
                Capacity = program.Capacity,
                IsActive = program.IsActive,
                IsApprovedByBtecAuthority = program.IsApprovedByBtecAuthority,
                ApprovalDate = program.ApprovalDate,
                ApprovalNotes = program.ApprovalNotes,
                CreatedAt = program.CreatedAt,
                UpdatedAt = program.UpdatedAt,
                UniversityNameArabic = program.University.NameArabic,
                UniversityNameEnglish = program.University.NameEnglish,
                EntryRequirements = program.EntryRequirements.Select(er => new BtecEntryRequirementDto
                {
                    Id = er.Id,
                    MinGPA = er.MinGPA,
                    RequiresBtecL2 = er.RequiresBtecL2,
                    RequiresBtecL3 = er.RequiresBtecL3,
                    Notes = er.Notes,
                    EffectiveFrom = er.EffectiveFrom,
                    EffectiveTo = er.EffectiveTo
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/BTECPrograms/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["ErrorMessage"] = "You do not have permission to create BTEC programs.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateBtecProgramViewModel());
        }

        // POST: /UniversityRep/BTECPrograms/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBtecProgramViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["ErrorMessage"] = "You do not have permission to create BTEC programs.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var btecProgram = new BtecProgram
            {
                UniversityId = rep.UniversityId,
                NameArabic = model.NameArabic,
                NameEnglish = model.NameEnglish,
                Description = model.Description,
                Level = model.Level,
                TechnicalField = model.TechnicalField,
                Language = model.Language,
                DurationInYears = model.DurationInYears,
                SemesterStartDate = model.SemesterStartDate,
                TotalCreditHours = model.TotalCreditHours,
                HourPriceBase = model.HourPriceBase,
                RegistrationFeeFirstSemester = model.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = model.RegistrationFeeRegularSemester,
                Capacity = model.Capacity,
                IsActive = true,
                IsApprovedByBtecAuthority = false, // Requires approval
                CreatedAt = DateTime.UtcNow
            };

            _context.BtecPrograms.Add(btecProgram);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "BTEC program created successfully. Waiting for BTEC Authority approval.";
            return RedirectToAction(nameof(Details), new { id = btecProgram.Id });
        }

        // GET: /UniversityRep/BTECPrograms/{id}/Edit
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit BTEC programs.";
                return RedirectToAction(nameof(Index));
            }

            var program = await _context.BtecPrograms
                .FirstOrDefaultAsync(bp => bp.Id == id && bp.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["ErrorMessage"] = "BTEC program not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditBtecProgramViewModel
            {
                Id = program.Id,
                NameArabic = program.NameArabic,
                NameEnglish = program.NameEnglish,
                Description = program.Description,
                Level = program.Level,
                TechnicalField = program.TechnicalField,
                Language = program.Language,
                DurationInYears = program.DurationInYears,
                SemesterStartDate = program.SemesterStartDate,
                TotalCreditHours = program.TotalCreditHours,
                HourPriceBase = program.HourPriceBase,
                RegistrationFeeFirstSemester = program.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = program.RegistrationFeeRegularSemester,
                Capacity = program.Capacity,
                IsApprovedByBtecAuthority = program.IsApprovedByBtecAuthority
            };

            return View(model);
        }

        // POST: /UniversityRep/BTECPrograms/{id}/Edit
        [HttpPost("{id}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBtecProgramViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit BTEC programs.";
                return RedirectToAction(nameof(Index));
            }

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var program = await _context.BtecPrograms
                .FirstOrDefaultAsync(bp => bp.Id == id && bp.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["ErrorMessage"] = "BTEC program not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if fees can be managed
            if (!rep.CanManageFees)
            {
                // Prevent fee changes
                model.HourPriceBase = program.HourPriceBase;
                model.RegistrationFeeFirstSemester = program.RegistrationFeeFirstSemester;
                model.RegistrationFeeRegularSemester = program.RegistrationFeeRegularSemester;
            }

            program.NameArabic = model.NameArabic;
            program.NameEnglish = model.NameEnglish;
            program.Description = model.Description;
            program.Level = model.Level;
            program.TechnicalField = model.TechnicalField;
            program.Language = model.Language;
            program.DurationInYears = model.DurationInYears;
            program.SemesterStartDate = model.SemesterStartDate;
            program.TotalCreditHours = model.TotalCreditHours;
            program.HourPriceBase = model.HourPriceBase;
            program.RegistrationFeeFirstSemester = model.RegistrationFeeFirstSemester;
            program.RegistrationFeeRegularSemester = model.RegistrationFeeRegularSemester;
            program.Capacity = model.Capacity;
            program.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "BTEC program updated successfully.";
            return RedirectToAction(nameof(Details), new { id = program.Id });
        }

        // POST: /UniversityRep/BTECPrograms/{id}/Toggle
        [HttpPost("{id}/Toggle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var program = await _context.BtecPrograms
                .FirstOrDefaultAsync(bp => bp.Id == id && bp.UniversityId == rep.UniversityId);

            if (program == null)
            {
                return Json(new { success = false, message = "BTEC program not found." });
            }

            program.IsActive = !program.IsActive;
            program.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = program.IsActive,
                message = $"BTEC program {(program.IsActive ? "activated" : "deactivated")} successfully."
            });
        }

        // GET: /UniversityRep/BTECPrograms/{id}/EntryRequirements
        [HttpGet("{id}/EntryRequirements")]
        public async Task<IActionResult> EntryRequirements(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null)
            {
                TempData["ErrorMessage"] = "You are not assigned to any university.";
                return RedirectToAction("Index", "UniversityRep");
            }

            var program = await _context.BtecPrograms
                .Include(bp => bp.EntryRequirements)
                .FirstOrDefaultAsync(bp => bp.Id == id && bp.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["ErrorMessage"] = "BTEC program not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new BtecEntryRequirementsViewModel
            {
                ProgramId = program.Id,
                ProgramNameArabic = program.NameArabic,
                ProgramNameEnglish = program.NameEnglish,
                CanManagePrograms = rep.CanManagePrograms,
                EntryRequirements = program.EntryRequirements.Select(er => new BtecEntryRequirementDto
                {
                    Id = er.Id,
                    MinGPA = er.MinGPA,
                    RequiresBtecL2 = er.RequiresBtecL2,
                    RequiresBtecL3 = er.RequiresBtecL3,
                    Notes = er.Notes,
                    EffectiveFrom = er.EffectiveFrom,
                    EffectiveTo = er.EffectiveTo
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /UniversityRep/BTECPrograms/{id}/EntryRequirements/Add
        [HttpPost("{id}/EntryRequirements/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntryRequirement(int id, AddBtecEntryRequirementViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var program = await _context.BtecPrograms
                .FirstOrDefaultAsync(bp => bp.Id == id && bp.UniversityId == rep.UniversityId);

            if (program == null)
            {
                return Json(new { success = false, message = "BTEC program not found." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            var entryRequirement = new BtecEntryRequirement
            {
                BtecProgramId = id,
                MinGPA = model.MinGPA,
                RequiresBtecL2 = model.RequiresBtecL2,
                RequiresBtecL3 = model.RequiresBtecL3,
                Notes = model.Notes,
                EffectiveFrom = model.EffectiveFrom ?? DateTime.UtcNow
            };

            _context.BtecEntryRequirements.Add(entryRequirement);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Entry requirement added successfully." });
        }

        // POST: /UniversityRep/BTECPrograms/{id}/EntryRequirements/{reqId}/Delete
        [HttpPost("{id}/EntryRequirements/{reqId}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntryRequirement(int id, int reqId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var entryRequirement = await _context.BtecEntryRequirements
                .Include(er => er.BtecProgram)
                .FirstOrDefaultAsync(er => er.Id == reqId &&
                    er.BtecProgramId == id &&
                    er.BtecProgram.UniversityId == rep.UniversityId);

            if (entryRequirement == null)
            {
                return Json(new { success = false, message = "Entry requirement not found." });
            }

            _context.BtecEntryRequirements.Remove(entryRequirement);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Entry requirement deleted successfully." });
        }
    }
}
