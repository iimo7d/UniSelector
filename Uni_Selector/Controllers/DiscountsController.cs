using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.Service.Interface;
using Uni_Selector.ViewModels.Discounts;

namespace Uni_Selector.Controllers
{
    [Authorize(Roles = UserRoles.Student)]
    [Route("[controller]")]
    public class DiscountsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public DiscountsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }


        [HttpGet("")]
        public async Task<IActionResult> Index(string status, int pageNumber = 1, int pageSize = 12)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                TempData["Warning"] = "Please complete your profile first.";
                return RedirectToAction("CompleteProfile", "Student");
            }

            // Get discounts query
            var query = _context.DiscountGrants
                .Include(d => d.Application)
                    .ThenInclude(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                .Include(d => d.Application)
                    .ThenInclude(a => a.BtecProgram)
                .Include(d => d.University)
                .Where(d => d.Application.StudentId == student.Id);

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<DiscountStatus>(status, out var statusEnum))
            {
                query = query.Where(d => d.Status == statusEnum);
            }

            // Order by granted date (newest first)
            query = query.OrderByDescending(d => d.GrantedAt);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var discounts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new DiscountsListViewModel
            {
                Discounts = discounts.Select(d => new DiscountItemViewModel
                {
                    Id = d.Id,
                    Code = d.Code,
                    Percentage = d.Percentage,
                    AmountEstimated = d.AmountEstimated,
                    Status = d.Status,
                    GrantedAt = d.GrantedAt,
                    RedeemedAt = d.RedeemedAt,
                    UniversityName = d.University.NameEnglish,
                    UniversityLogo = d.University.LogoPath,
                    ProgramName = d.Application.UniversityProgram != null
                        ? d.Application.UniversityProgram.Program.NameEnglish
                        : d.Application.BtecProgram.NameEnglish,
                    ApplicationNumber = d.Application.ApplicationNumber,
                    IsValid = d.Status == DiscountStatus.Issued
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                StatusFilter = status
            };

            // Statistics
            ViewBag.TotalDiscounts = totalCount;
            ViewBag.IssuedCount = await _context.DiscountGrants
                .Where(d => d.Application.StudentId == student.Id && d.Status == DiscountStatus.Issued)
                .CountAsync();
            ViewBag.RedeemedCount = await _context.DiscountGrants
                .Where(d => d.Application.StudentId == student.Id && d.Status == DiscountStatus.Redeemed)
                .CountAsync();
            ViewBag.ExpiredCount = await _context.DiscountGrants
                .Where(d => d.Application.StudentId == student.Id && d.Status == DiscountStatus.Expired)
                .CountAsync();

            return View(model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            var discount = await _context.DiscountGrants
                .Include(d => d.Application)
                    .ThenInclude(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                .Include(d => d.Application)
                    .ThenInclude(a => a.UniversityProgram)
                        .ThenInclude(up => up.University)
                .Include(d => d.Application)
                    .ThenInclude(a => a.BtecProgram)
                        .ThenInclude(bp => bp.University)
                .Include(d => d.University)
                .Include(d => d.RedeemedByUser)
                .FirstOrDefaultAsync(d => d.Id == id && d.Application.StudentId == student.Id);

            if (discount == null)
                return NotFound();

            var model = new DiscountDetailsViewModel
            {
                Id = discount.Id,
                Code = discount.Code,
                Percentage = discount.Percentage,
                AmountEstimated = discount.AmountEstimated,
                Status = discount.Status,
                GrantedAt = discount.GrantedAt,
                RedeemedAt = discount.RedeemedAt,
                RedeemedBy = discount.RedeemedByUser?.FullName,
                UniversityName = discount.University.NameEnglish,
                UniversityNameArabic = discount.University.NameArabic,
                UniversityLogo = discount.University.LogoPath,
                UniversityAddress = discount.University.FullAddress,
                UniversityPhone = discount.University.PhoneNumber,
                UniversityEmail = discount.University.Email,
                ProgramName = discount.Application.UniversityProgram != null
                    ? discount.Application.UniversityProgram.Program.NameEnglish
                    : discount.Application.BtecProgram.NameEnglish,
                ProgramNameArabic = discount.Application.UniversityProgram != null
                    ? discount.Application.UniversityProgram.Program.NameArabic
                    : discount.Application.BtecProgram.NameArabic,
                ApplicationNumber = discount.Application.ApplicationNumber,
                ApplicationStatus = discount.Application.Status,
                StudentFullName = student.User.FullName,
                StudentEmail = student.User.Email,
                IsValid = discount.Status == DiscountStatus.Issued,
                DaysUntilExpiry = discount.Status == DiscountStatus.Issued
                    ? (discount.GrantedAt.AddDays(90) - DateTime.UtcNow).Days
                    : 0
            };

            // Get registration fee for estimated savings
            if (discount.Application.UniversityProgram != null)
            {
                model.EstimatedRegistrationFee = discount.Application.UniversityProgram.RegistrationFeeFirstSemester;
            }
            else if (discount.Application.BtecProgram != null)
            {
                model.EstimatedRegistrationFee = discount.Application.BtecProgram.RegistrationFeeFirstSemester;
            }

            return View(model);
        }

        // GET: /Discounts/{id}/Download
        [HttpGet("{id}/Download")]
        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            var discount = await _context.DiscountGrants
                .Include(d => d.Application)
                    .ThenInclude(a => a.UniversityProgram)
                        .ThenInclude(up => up.Program)
                .Include(d => d.Application)
                    .ThenInclude(a => a.BtecProgram)
                .Include(d => d.University)
                .FirstOrDefaultAsync(d => d.Id == id && d.Application.StudentId == student.Id);

            if (discount == null)
                return NotFound();

            // Generate PDF
            var pdfBytes = GenerateDiscountCertificatePdf(discount, student);

            var fileName = $"Discount_Certificate_{discount.Code}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // POST: /Discounts/{id}/Verify
        [HttpPost("{id}/Verify")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return Json(new { success = false, message = "Student not found" });

            var discount = await _context.DiscountGrants
                .Include(d => d.University)
                .FirstOrDefaultAsync(d => d.Id == id && d.Application.StudentId == student.Id);

            if (discount == null)
                return Json(new { success = false, message = "Discount not found" });

            // Check if discount is valid
            var isValid = discount.Status == DiscountStatus.Issued;
            var expiryDate = discount.GrantedAt.AddDays(90);
            var isExpired = DateTime.UtcNow > expiryDate;

            if (isExpired && discount.Status == DiscountStatus.Issued)
            {
                // Auto-expire the discount
                discount.Status = DiscountStatus.Expired;
                await _context.SaveChangesAsync();
                isValid = false;
            }

            return Json(new
            {
                success = true,
                isValid = isValid,
                status = discount.Status.ToString(),
                code = discount.Code,
                percentage = discount.Percentage,
                university = discount.University.NameEnglish,
                grantedAt = discount.GrantedAt.ToString("MMM dd, yyyy"),
                expiresAt = expiryDate.ToString("MMM dd, yyyy"),
                redeemedAt = discount.RedeemedAt?.ToString("MMM dd, yyyy"),
                daysRemaining = isValid ? Math.Max(0, (expiryDate - DateTime.UtcNow).Days) : 0,
                message = isValid
                    ? "Discount code is valid and ready to use!"
                    : $"Discount code is {discount.Status.ToString().ToLower()}"
            });
        }

        #region Helper Methods

        private byte[] GenerateDiscountCertificatePdf(DiscountGrant discount, Student student)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Colors
                var primaryColor = new BaseColor(79, 70, 229); // Primary blue
                var greenColor = new BaseColor(34, 197, 94);
                var grayColor = new BaseColor(107, 114, 128);

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 28, primaryColor);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, primaryColor);
                var subheaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
                var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, grayColor);
                var codeFont = FontFactory.GetFont(FontFactory.COURIER_BOLD, 24, greenColor);

                // Title
                var title = new Paragraph("DISCOUNT CERTIFICATE", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(title);

                var subtitle = new Paragraph("Uni_Selector Platform", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };
                document.Add(subtitle);

                // Horizontal line
                var line = new Paragraph("_________________________________________________________________")
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(line);

                // Discount Code Box
                var codeHeader = new Paragraph("Your Discount Code", subheaderFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(codeHeader);

                var codePara = new Paragraph(discount.Code, codeFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(codePara);

                var percentPara = new Paragraph($"{discount.Percentage}% Discount", headerFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };
                document.Add(percentPara);

                // Details Table
                var table = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 20,
                    SpacingAfter = 20
                };
                table.SetWidths(new float[] { 40, 60 });

                // Helper method to add rows
                void AddTableRow(string label, string value)
                {
                    var labelCell = new PdfPCell(new Phrase(label, subheaderFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 10,
                        PaddingTop = 5
                    };
                    var valueCell = new PdfPCell(new Phrase(value, normalFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 10,
                        PaddingTop = 5
                    };
                    table.AddCell(labelCell);
                    table.AddCell(valueCell);
                }

                AddTableRow("Student Name:", student.User.FullName);
                AddTableRow("Student Email:", student.User.Email);
                AddTableRow("University:", discount.University.NameEnglish);

                var programName = discount.Application.UniversityProgram != null
                    ? discount.Application.UniversityProgram.Program.NameEnglish
                    : discount.Application.BtecProgram.NameEnglish;
                AddTableRow("Program:", programName);

                AddTableRow("Application Number:", discount.Application.ApplicationNumber ?? "N/A");
                AddTableRow("Discount Percentage:", $"{discount.Percentage}%");
                AddTableRow("Estimated Savings:", $"{discount.AmountEstimated:N2} JOD");
                AddTableRow("Status:", discount.Status.ToString());
                AddTableRow("Issued On:", discount.GrantedAt.ToString("MMMM dd, yyyy"));
                AddTableRow("Expires On:", discount.GrantedAt.AddDays(90).ToString("MMMM dd, yyyy"));

                document.Add(table);

                // Instructions
                document.Add(new Paragraph("_________________________________________________________________")
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                });

                var instructionsHeader = new Paragraph("How to Use This Discount", subheaderFont)
                {
                    SpacingAfter = 10
                };
                document.Add(instructionsHeader);

                var instructions = new iTextSharp.text.List(iTextSharp.text.List.ORDERED);
                instructions.SetListSymbol("\u2022");
                instructions.IndentationLeft = 20f;

                instructions.Add(new ListItem("Present this certificate to the university admissions office during registration", normalFont));
                instructions.Add(new ListItem("Provide the discount code shown above", normalFont));
                instructions.Add(new ListItem("The university will verify and apply the discount to your tuition fees", normalFont));
                instructions.Add(new ListItem("This discount is valid for 90 days from the issue date", normalFont));
                instructions.Add(new ListItem("This discount can only be used once", normalFont));

                document.Add(instructions);

                // Footer
                document.Add(new Paragraph("_________________________________________________________________")
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 30,
                    SpacingAfter = 10
                });

                var footer = new Paragraph(
                    $"Generated on {DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm} UTC\n" +
                    "Uni_Selector Platform - Smart University Selection System\n" +
                    "For support, contact: support@uniselector.jo",
                    smallFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(footer);

                document.Close();
                return ms.ToArray();
            }
        }
        #endregion
    }


}

