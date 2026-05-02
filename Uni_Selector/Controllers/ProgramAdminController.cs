using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.AdminProgram;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.PlatformAdmin)]
    public class ProgramAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProgramAdminController> _logger;

        public ProgramAdminController(AppDbContext context, ILogger<ProgramAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchTerm = null,
           Degree? degree = null, LanguageCode? language = null)
        {
            try
            {
                var query = _context.Programs
                    .Include(p => p.UniversityPrograms)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.NameEnglish.Contains(searchTerm) ||
                        p.NameArabic.Contains(searchTerm) ||
                        (p.AcademicClassification != null && p.AcademicClassification.Contains(searchTerm)));
                }

                // Apply degree filter
                if (degree.HasValue)
                {
                    query = query.Where(p => p.Degree == degree.Value);
                }

                // Apply language filter
                if (language.HasValue)
                {
                    query = query.Where(p => p.Language == language.Value);
                }

                // Get total count
                var totalPrograms = await query.CountAsync();

                // Apply pagination
                var programs = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProgramListItemViewModel
                    {
                        Id = p.Id,
                        NameEnglish = p.NameEnglish,
                        NameArabic = p.NameArabic,
                        DegreeText = p.Degree.ToString(),
                        LanguageText = p.Language.ToString(),
                        AcademicClassification = p.AcademicClassification,
                        TotalCreditHours = p.TotalCreditHours,
                        UniversitiesCount = p.UniversityPrograms.Count,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                var viewModel = new ProgramListViewModel
                {
                    Programs = programs,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPrograms = totalPrograms,
                    SearchTerm = searchTerm,
                    Degree = degree,
                    Language = language,
                    Degrees = Enum.GetValues(typeof(Degree)).Cast<Degree>().ToList(),
                    Languages = Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>().ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading programs list");
                TempData["Error"] = "An error occurred while loading the programs.";
                return View(new ProgramListViewModel());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProgramCreateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProgramCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check for duplicate program name
                var existingProgram = await _context.Programs
                    .FirstOrDefaultAsync(p =>
                        p.NameEnglish == model.NameEnglish ||
                        p.NameArabic == model.NameArabic);

                if (existingProgram != null)
                {
                    ModelState.AddModelError("", "A program with this name already exists.");
                    return View(model);
                }

                var program = new ProgramEntity
                {
                    NameArabic = model.NameArabic,
                    NameEnglish = model.NameEnglish,
                    Description = model.Description,
                    Degree = model.Degree,
                    Language = model.Language,
                    AcademicClassification = model.AcademicClassification,
                    TotalCreditHours = model.TotalCreditHours,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Programs.Add(program);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Program created: {program.NameEnglish} (ID: {program.Id})");
                TempData["Success"] = $"Program '{program.NameEnglish}' has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating program");
                ModelState.AddModelError("", "An error occurred while creating the program.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var program = await _context.Programs
                    .Include(p => p.UniversityPrograms)
                        .ThenInclude(up => up.University)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (program == null)
                {
                    TempData["Error"] = "Program not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ProgramDetailsViewModel
                {
                    Id = program.Id,
                    NameArabic = program.NameArabic,
                    NameEnglish = program.NameEnglish,
                    Description = program.Description,
                    DegreeText = program.Degree.ToString(),
                    LanguageText = program.Language.ToString(),
                    AcademicClassification = program.AcademicClassification,
                    TotalCreditHours = program.TotalCreditHours,
                    CreatedAt = program.CreatedAt,
                    UniversitiesOffering = program.UniversityPrograms
                        .Select(up => new UniversityOfferingViewModel
                        {
                            UniversityId = up.UniversityId,
                            UniversityNameEnglish = up.University.NameEnglish,
                            UniversityNameArabic = up.University.NameArabic,
                            UniversityLogoPath = up.University.LogoPath,
                            StudySystem = up.StudySystem.ToString(),
                            DurationInYears = up.DurationInYears,
                            HourPriceBase = up.HourPriceBase,
                            RegistrationFeeFirstSemester = up.RegistrationFeeFirstSemester,
                            IsActive = up.IsActive
                        })
                        .ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading program details for ID: {id}");
                TempData["Error"] = "An error occurred while loading the program details.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var program = await _context.Programs.FindAsync(id);

                if (program == null)
                {
                    TempData["Error"] = "Program not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ProgramCreateEditViewModel
                {
                    Id = program.Id,
                    NameArabic = program.NameArabic,
                    NameEnglish = program.NameEnglish,
                    Description = program.Description,
                    Degree = program.Degree,
                    Language = program.Language,
                    AcademicClassification = program.AcademicClassification,
                    TotalCreditHours = program.TotalCreditHours,
                    CreatedAt = program.CreatedAt
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for program ID: {id}");
                TempData["Error"] = "An error occurred while loading the program details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProgramCreateEditViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["Error"] = "Invalid program ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var program = await _context.Programs.FindAsync(id);
                if (program == null)
                {
                    TempData["Error"] = "Program not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check for duplicate program name (excluding current program)
                var duplicateProgram = await _context.Programs
                    .FirstOrDefaultAsync(p =>
                        p.Id != id &&
                        (p.NameEnglish == model.NameEnglish || p.NameArabic == model.NameArabic));

                if (duplicateProgram != null)
                {
                    ModelState.AddModelError("", "A program with this name already exists.");
                    return View(model);
                }

                // Update program properties
                program.NameArabic = model.NameArabic;
                program.NameEnglish = model.NameEnglish;
                program.Description = model.Description;
                program.Degree = model.Degree;
                program.Language = model.Language;
                program.AcademicClassification = model.AcademicClassification;
                program.TotalCreditHours = model.TotalCreditHours;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Program updated: {program.NameEnglish} (ID: {program.Id})");
                TempData["Success"] = $"Program '{program.NameEnglish}' has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating program ID: {id}");
                ModelState.AddModelError("", "An error occurred while updating the program.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var program = await _context.Programs
                    .Include(p => p.UniversityPrograms)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (program == null)
                {
                    TempData["Error"] = "Program not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if program is being used by any university
                if (program.UniversityPrograms.Any())
                {
                    TempData["Error"] = $"Cannot delete '{program.NameEnglish}' because it is being offered by {program.UniversityPrograms.Count} university(ies). Please remove it from all universities first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Programs.Remove(program);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Program deleted: {program.NameEnglish} (ID: {program.Id})");
                TempData["Success"] = $"Program '{program.NameEnglish}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting program ID: {id}");
                TempData["Error"] = "An error occurred while deleting the program.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return View();
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                TempData["Error"] = "Invalid file format. Please upload an Excel file (.xlsx or .xls).";
                return View();
            }

            var result = new ProgramImportResultViewModel();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                // Use EPPlus to read Excel file
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using var package = new OfficeOpenXml.ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0]; // First sheet

                // Check if worksheet has data
                if (worksheet.Dimension == null)
                {
                    TempData["Error"] = "The Excel file is empty.";
                    return View();
                }

                int rowCount = worksheet.Dimension.Rows;

                // Validate headers (row 1)
                var expectedHeaders = new[] { "Name (English)", "Name (Arabic)", "Description", "Degree", "Language", "Academic Classification", "Total Credit Hours" };
                for (int col = 1; col <= expectedHeaders.Length; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                    if (headerValue != expectedHeaders[col - 1])
                    {
                        TempData["Error"] = $"Invalid Excel format. Expected header '{expectedHeaders[col - 1]}' in column {col}.";
                        return View();
                    }
                }

                // Process each row (starting from row 2)
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var nameEnglish = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var nameArabic = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var description = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var degreeStr = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                        var languageStr = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                        var academicClassification = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                        var creditHoursStr = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

                        // Skip empty rows
                        if (string.IsNullOrEmpty(nameEnglish) && string.IsNullOrEmpty(nameArabic))
                            continue;

                        // Validate required fields
                        if (string.IsNullOrEmpty(nameEnglish))
                        {
                            result.Errors.Add($"Row {row}: English name is required.");
                            result.ErrorCount++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(nameArabic))
                        {
                            result.Errors.Add($"Row {row}: Arabic name is required.");
                            result.ErrorCount++;
                            continue;
                        }

                        // Parse and validate Degree
                        if (!Enum.TryParse<Degree>(degreeStr, true, out var degree))
                        {
                            result.Errors.Add($"Row {row}: Invalid degree '{degreeStr}'. Valid values: Diploma, Bachelor, Master, PhD.");
                            result.ErrorCount++;
                            continue;
                        }

                        // Parse and validate Language
                        if (!Enum.TryParse<LanguageCode>(languageStr, true, out var language))
                        {
                            result.Errors.Add($"Row {row}: Invalid language '{languageStr}'. Valid values: English, Arabic, Both.");
                            result.ErrorCount++;
                            continue;
                        }

                        // Parse and validate Credit Hours
                        if (!int.TryParse(creditHoursStr, out var creditHours) || creditHours < 30 || creditHours > 300)
                        {
                            result.Errors.Add($"Row {row}: Invalid credit hours '{creditHoursStr}'. Must be between 30 and 300.");
                            result.ErrorCount++;
                            continue;
                        }

                        // Check for duplicates in database
                        var existingProgram = await _context.Programs
                            .FirstOrDefaultAsync(p => p.NameEnglish == nameEnglish || p.NameArabic == nameArabic);

                        if (existingProgram != null)
                        {
                            result.SkippedPrograms.Add(new ProgramImportItemViewModel
                            {
                                NameEnglish = nameEnglish,
                                NameArabic = nameArabic,
                                Degree = degree.ToString(),
                                Language = language.ToString(),
                                TotalCreditHours = creditHours,
                                Reason = "Program with this name already exists"
                            });
                            result.DuplicateCount++;
                            continue;
                        }

                        // Create new program
                        var program = new ProgramEntity
                        {
                            NameEnglish = nameEnglish,
                            NameArabic = nameArabic,
                            Description = string.IsNullOrEmpty(description) ? null : description,
                            Degree = degree,
                            Language = language,
                            AcademicClassification = string.IsNullOrEmpty(academicClassification) ? null : academicClassification,
                            TotalCreditHours = creditHours,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Programs.Add(program);

                        result.ImportedPrograms.Add(new ProgramImportItemViewModel
                        {
                            NameEnglish = nameEnglish,
                            NameArabic = nameArabic,
                            Degree = degree.ToString(),
                            Language = language.ToString(),
                            TotalCreditHours = creditHours
                        });
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {row}: {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                // Save all changes
                if (result.SuccessCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully imported {result.SuccessCount} programs from Excel file.");
                }

                // Store result in TempData for display
                TempData["ImportResult"] = System.Text.Json.JsonSerializer.Serialize(result);
                return RedirectToAction("ImportResult");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing programs from Excel");
                TempData["Error"] = $"An error occurred while processing the Excel file: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult ImportResult()
        {
            if (TempData["ImportResult"] == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var resultJson = TempData["ImportResult"]?.ToString();
            var result = System.Text.Json.JsonSerializer.Deserialize<ProgramImportResultViewModel>(resultJson);
            return View(result);
        }
    }

}
