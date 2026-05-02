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
    [Route("UniversityRep/Programs")]
    public class ProgramManagementController : Controller
    {
        private readonly AppDbContext _context;

        public ProgramManagementController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /UniversityRep/Programs
        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, bool? isActive, int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null)
            {
                TempData["Error"] = "You are not assigned to any university.";
                return RedirectToAction("Index", "UniversityRep");
            }

            // Check permissions
            if (!rep.CanManagePrograms)
            {
                TempData["Error"] = "You do not have permission to manage programs.";
                return RedirectToAction("Index", "UniversityRep");
            }

            var query = _context.UniversityPrograms
                .Include(up => up.Program)
                .Include(up => up.University)
                .Where(up => up.UniversityId == rep.UniversityId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(up =>
                    up.Program.NameArabic.Contains(search) ||
                    up.Program.NameEnglish.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(up => up.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var programs = await query
                .OrderByDescending(up => up.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(up => new ProgramListItemDto
                {
                    Id = up.Id,
                    ProgramNameArabic = up.Program.NameArabic,
                    ProgramNameEnglish = up.Program.NameEnglish,
                    Degree = up.Program.Degree,
                    Language = up.Program.Language,
                    StudySystem = up.StudySystem,
                    DurationInYears = up.DurationInYears,
                    HourPriceBase = up.HourPriceBase,
                    Capacity = up.Capacity,
                    IsActive = up.IsActive,
                    CreatedAt = up.CreatedAt
                })
                .ToListAsync();

            var viewModel = new ProgramListViewModel
            {
                Programs = programs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Search = search,
                IsActiveFilter = isActive,
                UniversityId = rep.UniversityId,
                CanManagePrograms = rep.CanManagePrograms
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/Programs/{id}/Details
        [HttpGet("{id}/Details")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null)
            {
                TempData["Error"] = "You are not assigned to any university.";
                return RedirectToAction("Index", "UniversityRep");
            }

            var program = await _context.UniversityPrograms
                .Include(up => up.Program)
                .Include(up => up.University)
                .Include(up => up.EntryRequirements)
                .FirstOrDefaultAsync(up => up.Id == id && up.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["Error"] = "Program not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ProgramDetailsViewModel
            {
                Id = program.Id,
                ProgramNameArabic = program.Program.NameArabic,
                ProgramNameEnglish = program.Program.NameEnglish,
                Description = program.Program.Description,
                Degree = program.Program.Degree,
                Language = program.Program.Language,
                TotalCreditHours = program.Program.TotalCreditHours,
                StudySystem = program.StudySystem,
                DurationInYears = program.DurationInYears,
                SemesterStartDate = program.SemesterStartDate,
                HourPriceBase = program.HourPriceBase,
                RegistrationFeeFirstSemester = program.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = program.RegistrationFeeRegularSemester,
                Capacity = program.Capacity,
                IsActive = program.IsActive,
                CreatedAt = program.CreatedAt,
                UpdatedAt = program.UpdatedAt,
                UniversityNameArabic = program.University.NameArabic,
                UniversityNameEnglish = program.University.NameEnglish,
                EntryRequirements = program.EntryRequirements.Select(er => new EntryRequirementDto
                {
                    Id = er.Id,
                    MinGPA = er.MinGPA,
                    Path = er.Path,
                    AcademicTrack = er.AcademicTrack,
                    VocationalBranch = er.VocationalBranch,
                    AllowNoTawjihi = er.AllowNoTawjihi,
                    Notes = er.Notes,
                    EffectiveFrom = er.EffectiveFrom,
                    EffectiveTo = er.EffectiveTo
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: /UniversityRep/Programs/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["Error"] = "You do not have permission to create programs.";
                return RedirectToAction(nameof(Index));
            }

            // Get available programs not yet added to this university
            var existingProgramIds = await _context.UniversityPrograms
                .Where(up => up.UniversityId == rep.UniversityId)
                .Select(up => up.ProgramId)
                .ToListAsync();

            var availablePrograms = await _context.Programs
                .Where(p => !existingProgramIds.Contains(p.Id))
                .OrderBy(p => p.NameEnglish)
                .ToListAsync();

            ViewBag.AvailablePrograms = availablePrograms;

            return View(new CreateProgramViewModel());
        }

        // POST: /UniversityRep/Programs/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProgramViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["Error"] = "You do not have permission to create programs.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                // Reload available programs
                var existingProgramIds = await _context.UniversityPrograms
                    .Where(up => up.UniversityId == rep.UniversityId)
                    .Select(up => up.ProgramId)
                    .ToListAsync();

                var availablePrograms = await _context.Programs
                    .Where(p => !existingProgramIds.Contains(p.Id))
                    .OrderBy(p => p.NameEnglish)
                    .ToListAsync();

                ViewBag.AvailablePrograms = availablePrograms;
                return View(model);
            }

            // Validate that the selected Program exists
            var programExists = await _context.Programs.AnyAsync(p => p.Id == model.ProgramId);
            if (!programExists)
            {
                ModelState.AddModelError("ProgramId", "Selected program does not exist.");
            }

            // Check if program already exists for this university
            var exists = await _context.UniversityPrograms
                .AnyAsync(up => up.UniversityId == rep.UniversityId && up.ProgramId == model.ProgramId);

            if (exists)
            {
                ModelState.AddModelError("", "This program already exists for your university.");
            }

            if (!ModelState.IsValid)
            {
                // Reload available programs dropdown before returning the view
                var existingProgramIdsForError = await _context.UniversityPrograms
                    .Where(up => up.UniversityId == rep.UniversityId)
                    .Select(up => up.ProgramId)
                    .ToListAsync();

                ViewBag.AvailablePrograms = await _context.Programs
                    .Where(p => !existingProgramIdsForError.Contains(p.Id))
                    .OrderBy(p => p.NameEnglish)
                    .ToListAsync();

                return View(model);
            }

            var universityProgram = new UniversityProgram
            {
                UniversityId = rep.UniversityId,
                ProgramId = model.ProgramId,
                StudySystem = model.StudySystem,
                DurationInYears = model.DurationInYears,
                SemesterStartDate = model.SemesterStartDate,
                HourPriceBase = model.HourPriceBase,
                RegistrationFeeFirstSemester = model.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = model.RegistrationFeeRegularSemester,
                Capacity = model.Capacity,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UniversityPrograms.Add(universityProgram);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Program created successfully.";
            return RedirectToAction(nameof(Details), new { id = universityProgram.Id });
        }

        // GET: /UniversityRep/Programs/{id}/Edit
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["Error"] = "You do not have permission to edit programs.";
                return RedirectToAction(nameof(Index));
            }

            var program = await _context.UniversityPrograms
                .Include(up => up.Program)
                .FirstOrDefaultAsync(up => up.Id == id && up.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["Error"] = "Program not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditProgramViewModel
            {
                Id = program.Id,
                ProgramNameArabic = program.Program.NameArabic,
                ProgramNameEnglish = program.Program.NameEnglish,
                StudySystem = program.StudySystem,
                DurationInYears = program.DurationInYears,
                SemesterStartDate = program.SemesterStartDate,
                HourPriceBase = program.HourPriceBase,
                RegistrationFeeFirstSemester = program.RegistrationFeeFirstSemester,
                RegistrationFeeRegularSemester = program.RegistrationFeeRegularSemester,
                Capacity = program.Capacity
            };

            return View(model);
        }

        // POST: /UniversityRep/Programs/{id}/Edit
        [HttpPost("{id}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProgramViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                TempData["Error"] = "You do not have permission to edit programs.";
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

            var program = await _context.UniversityPrograms
                .FirstOrDefaultAsync(up => up.Id == id && up.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["Error"] = "Program not found.";
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

            program.StudySystem = model.StudySystem;
            program.DurationInYears = model.DurationInYears;
            program.SemesterStartDate = model.SemesterStartDate;
            program.HourPriceBase = model.HourPriceBase;
            program.RegistrationFeeFirstSemester = model.RegistrationFeeFirstSemester;
            program.RegistrationFeeRegularSemester = model.RegistrationFeeRegularSemester;
            program.Capacity = model.Capacity;
            program.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Program updated successfully.";
            return RedirectToAction(nameof(Details), new { id = program.Id });
        }

        // POST: /UniversityRep/Programs/{id}/Toggle
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

            var program = await _context.UniversityPrograms
                .FirstOrDefaultAsync(up => up.Id == id && up.UniversityId == rep.UniversityId);

            if (program == null)
            {
                return Json(new { success = false, message = "Program not found." });
            }

            program.IsActive = !program.IsActive;
            program.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = program.IsActive,
                message = $"Program {(program.IsActive ? "activated" : "deactivated")} successfully."
            });
        }

        // GET: /UniversityRep/Programs/{id}/EntryRequirements
        [HttpGet("{id}/EntryRequirements")]
        public async Task<IActionResult> EntryRequirements(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null)
            {
                TempData["Error"] = "You are not assigned to any university.";
                return RedirectToAction("Index", "UniversityRep");
            }

            var program = await _context.UniversityPrograms
                .Include(up => up.Program)
                .Include(up => up.EntryRequirements)
                .FirstOrDefaultAsync(up => up.Id == id && up.UniversityId == rep.UniversityId);

            if (program == null)
            {
                TempData["Error"] = "Program not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new EntryRequirementsViewModel
            {
                ProgramId = program.Id,
                ProgramNameArabic = program.Program.NameArabic,
                ProgramNameEnglish = program.Program.NameEnglish,
                CanManagePrograms = rep.CanManagePrograms,
                EntryRequirements = program.EntryRequirements.Select(er => new EntryRequirementDto
                {
                    Id = er.Id,
                    MinGPA = er.MinGPA,
                    Path = er.Path,
                    AcademicTrack = er.AcademicTrack,
                    VocationalBranch = er.VocationalBranch,
                    AllowNoTawjihi = er.AllowNoTawjihi,
                    Notes = er.Notes,
                    EffectiveFrom = er.EffectiveFrom,
                    EffectiveTo = er.EffectiveTo
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /UniversityRep/Programs/{id}/EntryRequirements/Add
        [HttpPost("{id}/EntryRequirements/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntryRequirement(int id, AddEntryRequirementViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rep = await _context.UniversityRepresentatives
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (rep == null || !rep.CanManagePrograms)
            {
                return Json(new { success = false, message = "Permission denied." });
            }

            var program = await _context.UniversityPrograms
                .FirstOrDefaultAsync(up => up.Id == id && up.UniversityId == rep.UniversityId);

            if (program == null)
            {
                return Json(new { success = false, message = "Program not found." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            var entryRequirement = new EntryRequirement
            {
                UniversityProgramId = id,
                MinGPA = model.MinGPA,
                Path = model.Path,
                AcademicTrack = model.AcademicTrack,
                VocationalBranch = model.VocationalBranch,
                AllowNoTawjihi = model.AllowNoTawjihi,
                Notes = model.Notes,
                EffectiveFrom = model.EffectiveFrom ?? DateTime.UtcNow
            };

            _context.EntryRequirements.Add(entryRequirement);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Entry requirement added successfully." });
        }

        // POST: /UniversityRep/Programs/{id}/EntryRequirements/{reqId}/Delete
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

            var entryRequirement = await _context.EntryRequirements
                .Include(er => er.UniversityProgram)
                .FirstOrDefaultAsync(er => er.Id == reqId &&
                    er.UniversityProgramId == id &&
                    er.UniversityProgram.UniversityId == rep.UniversityId);

            if (entryRequirement == null)
            {
                return Json(new { success = false, message = "Entry requirement not found." });
            }

            _context.EntryRequirements.Remove(entryRequirement);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Entry requirement deleted successfully." });
        }
    }
}
