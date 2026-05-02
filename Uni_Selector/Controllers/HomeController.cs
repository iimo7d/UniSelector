using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.Home;

namespace Uni_Selector.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;


        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ENDPOINT: GET /Home/About - About the platform
        [HttpGet]
        public async Task<IActionResult> About()
        {
            try
            {
                // Get platform statistics from database
                var totalUniversities = await _context.Universities.CountAsync(u => u.IsActive);
                var totalPrograms = await _context.UniversityPrograms.CountAsync(p => p.IsActive);
                var totalBtecPrograms = await _context.BtecPrograms.CountAsync(p => p.IsActive && p.IsApprovedByBtecAuthority);
                var totalStudents = await _context.Students.CountAsync(s => s.IsActive == true);
                var totalApplications = await _context.StudentApplications.CountAsync();

                var viewModel = new AboutViewModel
                {
                    PlatformName = "Uni Selector",
                    Title = "Welcome to Jordan's #1 University Selection Platform",
                    Description = "Uni Selector is Jordan's premier university matching platform, designed to help students find their perfect academic fit. We connect ambitious students with top private universities across Jordan, offering comprehensive information, intelligent matching, and streamlined application processes.",

                    Features = new List<FeatureItem>
                    {
                        new FeatureItem
                        {
                            Icon = "icofont-graduate-alt",
                            Title = "Smart University Matching",
                            Description = "Our AI-powered recommendation engine analyzes your academic profile, preferences, and goals to match you with the best universities and programs tailored to your needs."
                        },
                        new FeatureItem
                        {
                            Icon = "icofont-chart-line-alt",
                            Title = "Comprehensive Program Database",
                            Description = "Access detailed information about hundreds of academic programs and BTEC certifications across all private universities in Jordan, including costs, requirements, and career prospects."
                        },
                        new FeatureItem
                        {
                            Icon = "icofont-money",
                            Title = "Automatic Discounts",
                            Description = "Every student gets an automatic 5% discount on their first semester registration fees, plus access to exclusive university-specific promotions and scholarships."
                        },
                        new FeatureItem
                        {
                            Icon = "icofont-file-document",
                            Title = "Streamlined Applications",
                            Description = "Apply to multiple universities with a single profile. Track all your applications in one place and receive real-time updates on your admission status."
                        },
                        new FeatureItem
                        {
                            Icon = "icofont-support",
                            Title = "Expert Guidance",
                            Description = "Get personalized support throughout your university selection journey. Our team helps you understand requirements, compare options, and make informed decisions."
                        },
                        new FeatureItem
                        {
                            Icon = "icofont-certificate-alt-1",
                            Title = "BTEC Integration",
                            Description = "Seamless support for BTEC qualification holders with dedicated programs, entry requirements, and pathways to higher education at approved universities."
                        }
                    },

                    Statistics = new List<StatItem>
                    {
                        new StatItem
                        {
                            Icon = "icofont-university",
                            Number = $"{totalUniversities}+",
                            Label = "Partner Universities",
                            Color = "primaryColor"
                        },
                        new StatItem
                        {
                            Icon = "icofont-book-alt",
                            Number = $"{totalPrograms + totalBtecPrograms}+",
                            Label = "Academic Programs",
                            Color = "secondaryColor"
                        },
                        new StatItem
                        {
                            Icon = "icofont-users-alt-4",
                            Number = $"{totalStudents}+",
                            Label = "Happy Students",
                            Color = "greencolor"
                        },
                        new StatItem
                        {
                            Icon = "icofont-paper",
                            Number = $"{totalApplications}+",
                            Label = "Applications Processed",
                            Color = "secondaryColor3"
                        }
                    },

                    Team = new TeamSection
                    {
                        Title = "Our Mission",
                        Description = "At Uni Selector, we believe that every student deserves access to quality higher education. Our mission is to democratize university selection in Jordan by providing transparent, comprehensive, and intelligent tools that empower students to make the best decisions for their future.",
                        Points = new List<string>
                        {
                            "Simplify the university search and application process for all Jordanian students",
                            "Provide accurate, up-to-date information about universities, programs, and costs",
                            "Match students with programs that align with their academic profiles and career goals",
                            "Support BTEC qualification holders in accessing higher education pathways",
                            "Offer financial benefits through automatic discounts and scholarship access",
                            "Build lasting partnerships between students and educational institutions"
                        }
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading About page");
                TempData["ErrorMessage"] = "An error occurred while loading the About page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ENDPOINT: GET /Home/FAQ - Frequently asked questions
        [HttpGet]
        public IActionResult Faq()
        {
            try
            {
                var viewModel = new FaqViewModel
                {
                    Title = "Frequently Asked Questions",
                    Description = "Find answers to common questions about Uni Selector platform, applications, and processes",

                    Categories = new List<FaqCategory>
                    {
                        new FaqCategory
                        {
                            CategoryName = "Getting Started",
                            CategoryIcon = "icofont-rocket-alt-2",
                            CategoryColor = "primaryColor",
                            Questions = new List<FaqItem>
                            {
                                new FaqItem
                                {
                                    Id = 1,
                                    Question = "What is Uni Selector?",
                                    Answer = "Uni Selector is Jordan's leading university matching platform that helps students find and apply to the best private universities based on their academic profile, preferences, and goals. We provide comprehensive information, AI-powered recommendations, and streamlined application processes."
                                },
                                new FaqItem
                                {
                                    Id = 2,
                                    Question = "How do I create an account?",
                                    Answer = "Creating an account is simple! Click on 'Get Started' or 'Register' in the top menu, fill in your basic information (name, email, phone number), create a password, and verify your email. Once registered, you can complete your student profile to start receiving university recommendations."
                                },
                                new FaqItem
                                {
                                    Id = 3,
                                    Question = "Is Uni Selector free to use?",
                                    Answer = "Yes! Creating an account, browsing universities, and receiving recommendations is completely free. Plus, every student automatically receives a 5% discount on their first semester registration fees when they apply through our platform."
                                },
                                new FaqItem
                                {
                                    Id = 4,
                                    Question = "Which universities are available on the platform?",
                                    Answer = "We partner with all major private universities in Jordan, including institutions in Amman, Zarqa, Irbid, and other cities. You can browse the complete list on our Universities page, along with detailed information about each institution's programs, facilities, and costs."
                                }
                            }
                        },

                        new FaqCategory
                        {
                            CategoryName = "Student Profile & Requirements",
                            CategoryIcon = "icofont-user-alt-7",
                            CategoryColor = "secondaryColor",
                            Questions = new List<FaqItem>
                            {
                                new FaqItem
                                {
                                    Id = 5,
                                    Question = "What information do I need to complete my profile?",
                                    Answer = "To get accurate recommendations, you'll need: your Tawjihi results (GPA and academic track), location preferences, budget constraints, desired field of study, and any BTEC qualifications if applicable. You'll also provide basic personal information and guardian contact details."
                                },
                                new FaqItem
                                {
                                    Id = 6,
                                    Question = "What is the minimum GPA required?",
                                    Answer = "Minimum GPA requirements vary by university and program. Most programs require between 60-75% for entry, with premium programs and medical fields typically requiring 80% or higher. Our recommendation engine will only show programs that match your GPA."
                                },
                                new FaqItem
                                {
                                    Id = 7,
                                    Question = "Can I apply with a BTEC qualification?",
                                    Answer = "Absolutely! We fully support BTEC qualification holders. Several universities offer specialized BTEC programs at Levels 5-8. You'll need to upload your BTEC certificates during profile completion, and our system will recommend appropriate programs based on your qualifications."
                                },
                                new FaqItem
                                {
                                    Id = 8,
                                    Question = "What if I have a vocational Tawjihi?",
                                    Answer = "Students with vocational Tawjihi (Industrial, Agricultural, Hotel Tourism, Home Economics) are welcome! Many universities accept vocational tracks for specific programs. Make sure to select your branch correctly in your profile to receive appropriate recommendations."
                                }
                            }
                        },

                        new FaqCategory
                        {
                            CategoryName = "Applications & Admissions",
                            CategoryIcon = "icofont-file-document",
                            CategoryColor = "greencolor",
                            Questions = new List<FaqItem>
                            {
                                new FaqItem
                                {
                                    Id = 9,
                                    Question = "How do I apply to universities?",
                                    Answer = "After completing your profile, browse recommended programs and click 'Apply Now' on any program that interests you. Complete the application form, upload required documents, and submit. Universities will review your application and respond through the platform."
                                },
                                new FaqItem
                                {
                                    Id = 10,
                                    Question = "Can I apply to multiple universities?",
                                    Answer = "Yes! You can apply to as many universities and programs as you like. We recommend applying to 3-5 programs to maximize your chances of admission. Track all your applications from your student dashboard."
                                },
                                new FaqItem
                                {
                                    Id = 11,
                                    Question = "How long does the application process take?",
                                    Answer = "Most universities review applications within 3-7 business days. You'll receive real-time notifications about your application status (Under Review, Approved, Rejected). Some universities may require additional documents or interviews."
                                },
                                new FaqItem
                                {
                                    Id = 12,
                                    Question = "What documents do I need to submit?",
                                    Answer = "Typically: Tawjihi certificate, national ID copy, passport photo, and guardian ID. BTEC applicants need their certification documents. Some programs may require additional materials like portfolios or recommendation letters."
                                },
                                new FaqItem
                                {
                                    Id = 13,
                                    Question = "What happens after I'm accepted?",
                                    Answer = "Once accepted, you'll receive your admission number and discount code (5% automatic discount). Universities will contact you directly for enrollment procedures, payment schedules, and orientation dates. All communications are tracked in your dashboard."
                                }
                            }
                        },

                        new FaqCategory
                        {
                            CategoryName = "Costs & Financial Benefits",
                            CategoryIcon = "icofont-money-bag",
                            CategoryColor = "secondaryColor3",
                            Questions = new List<FaqItem>
                            {
                                new FaqItem
                                {
                                    Id = 14,
                                    Question = "How does the automatic 5% discount work?",
                                    Answer = "Every student who applies through Uni Selector automatically receives a unique discount code worth 5% off their first semester registration fees. This discount is generated upon application approval and can be redeemed directly at the university."
                                },
                                new FaqItem
                                {
                                    Id = 15,
                                    Question = "Are there additional discounts available?",
                                    Answer = "Yes! Universities may offer additional discounts based on academic merit, early registration, sibling enrollment, or special circumstances. Some universities also offer hour-by-hour discounts (30-50%) for exceptional students. Check each university's program details for specific offers."
                                },
                                new FaqItem
                                {
                                    Id = 16,
                                    Question = "How are program costs calculated?",
                                    Answer = "Most programs charge per credit hour plus registration fees. For example, a program might charge 65 JD/hour with a 450 JD registration fee. A typical 15-credit semester would cost: (15 hours × 65 JD) + 450 JD registration = 1,425 JD before discounts."
                                },
                                new FaqItem
                                {
                                    Id = 17,
                                    Question = "Can I see total program costs before applying?",
                                    Answer = "Yes! Each program page shows the complete cost breakdown including per-hour rates, registration fees, total credit hours required, and estimated total program cost. Our recommendation engine also factors in your budget preferences."
                                }
                            }
                        },

                        new FaqCategory
                        {
                            CategoryName = "Technical Support",
                            CategoryIcon = "icofont-support-faq",
                            CategoryColor = "primaryColor",
                            Questions = new List<FaqItem>
                            {
                                new FaqItem
                                {
                                    Id = 18,
                                    Question = "I forgot my password. What should I do?",
                                    Answer = "Click 'Forgot Password' on the login page, enter your registered email address, and you'll receive a password reset link. Follow the instructions in the email to create a new password. If you don't receive the email, check your spam folder."
                                },
                                new FaqItem
                                {
                                    Id = 19,
                                    Question = "How do I update my profile information?",
                                    Answer = "Log in to your account and click on 'My Profile' or 'Settings' in the dashboard. You can update your contact information, academic records, preferences, and documents. Some information may require verification before changes take effect."
                                },
                                new FaqItem
                                {
                                    Id = 20,
                                    Question = "Who do I contact for help?",
                                    Answer = "For technical issues or platform questions, use the contact form or email support@uniselector.jo. For university-specific questions about programs or admissions, contact the university directly using the information on their profile page."
                                },
                                new FaqItem
                                {
                                    Id = 21,
                                    Question = "Is my personal information secure?",
                                    Answer = "Yes! We take data security seriously. All personal information is encrypted, stored securely, and never shared with third parties without your consent. Universities only receive information you explicitly submit in your applications."
                                }
                            }
                        }
                    }
                };

                // Calculate total questions
                viewModel.TotalQuestions = viewModel.Categories.Sum(c => c.Questions.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FAQ page");
                TempData["ErrorMessage"] = "An error occurred while loading the FAQ page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ENDPOINT: GET /Home/Universities - Browse all universities (public list)
        [HttpGet]
        public async Task<IActionResult> Universities(string search = "", string city = "")
        {
            try
            {
                var query = _context.Universities
                    .Where(u => u.IsActive)
                    .Include(u => u.UniversityPrograms)
                    .Include(u => u.BtecPrograms)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(u =>
                        u.NameArabic.ToLower().Contains(search) ||
                        u.NameEnglish.ToLower().Contains(search) ||
                        u.City.ToLower().Contains(search) ||
                        u.Province.ToLower().Contains(search)
                    );
                }

                // Apply city filter
                if (!string.IsNullOrWhiteSpace(city))
                {
                    city = city.Trim();
                    query = query.Where(u => u.City == city);
                }

                var universities = await query
                    .OrderBy(u => u.NameEnglish)
                    .ToListAsync();

                // Get available cities for filter
                var availableCities = await _context.Universities
                    .Where(u => u.IsActive)
                    .Select(u => u.City)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                var viewModel = new UniversitiesListViewModel
                {
                    Universities = universities.Select(u =>
                    {
                        var activePrograms = (u.UniversityPrograms ?? new List<UniversityProgram>())
                            .Where(p => p.IsActive)
                            .ToList();

                        return new UniversityCardViewModel
                        {
                            Id = u.Id,
                            NameArabic = u.NameArabic,
                            NameEnglish = u.NameEnglish,
                            City = u.City,
                            Province = u.Province,
                            LogoPath = u.LogoPath ?? "/assets/images/logo/logo_1.png",
                            ImagePath = u.ImagePath ?? "/assets/images/grid/grid_1.png",
                            ProgramsCount = activePrograms.Count,
                            BTECProgramsCount = u.BtecPrograms?.Count(p => p.IsActive) ?? 0,
                            MinHourPrice = activePrograms.Any() ? activePrograms.Min(p => p.HourPriceBase) : 0,
                            IsActive = u.IsActive
                        };
                    }).ToList(),

                    TotalUniversities = universities.Count,
                    SearchQuery = search,
                    SelectedCity = city,
                    AvailableCities = availableCities
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading universities list");
                TempData["ErrorMessage"] = "An error occurred while loading the universities list. Please try again.";
                return View(new UniversitiesListViewModel());
            }
        }

        // ENDPOINT: GET /Home/University/{id} - View university details
        public async Task<IActionResult> University(int id)
        {
            try
            {
                var university = await _context.Universities
                    .Where(u => u.Id == id && u.IsActive)
                    .Include(u => u.UniversityPrograms.Where(p => p.IsActive))
                        .ThenInclude(up => up.Program)
                    .Include(u => u.UniversityPrograms.Where(p => p.IsActive))
                        .ThenInclude(up => up.EntryRequirements)
                    .Include(u => u.BtecPrograms.Where(p => p.IsActive && p.IsApprovedByBtecAuthority))
                        .ThenInclude(bp => bp.EntryRequirements)
                    .FirstOrDefaultAsync();

                if (university == null)
                {
                    _logger.LogWarning($"University with ID {id} not found or inactive");
                    TempData["ErrorMessage"] = "الجامعة غير موجودة أو غير متاحة.";
                    return RedirectToAction(nameof(Universities));
                }

                var totalStudents = await _context.StudentApplications
                    .Where(a => a.Status == ApplicationStatus.Enrolled &&
                                ((a.UniversityProgramId.HasValue &&
                                  _context.UniversityPrograms.Any(up => up.Id == a.UniversityProgramId && up.UniversityId == id)) ||
                                 (a.BtecProgramId.HasValue &&
                                  _context.BtecPrograms.Any(bp => bp.Id == a.BtecProgramId && bp.UniversityId == id))))
                    .CountAsync();

                // FIX: Get active programs list once
                var activePrograms = university.UniversityPrograms?.Where(p => p.IsActive).ToList() ?? new List<UniversityProgram>();

                var viewModel = new UniversityDetailsViewModel
                {
                    Id = university.Id,
                    NameArabic = university.NameArabic,
                    NameEnglish = university.NameEnglish,
                    Type = university.Type,
                    Province = university.Province,
                    City = university.City,
                    FullAddress = university.FullAddress,
                    Latitude = university.Latitude,
                    Longitude = university.Longitude,
                    PhoneNumber = university.PhoneNumber,
                    Email = university.Email,
                    OfficialWebsite = university.OfficialWebsite,
                    LogoPath = university.LogoPath ?? "/assets/images/logo/logo_1.png",
                    ImagePath = university.ImagePath ?? "/assets/images/grid/grid_1.png",
                    AcademicAccreditation = university.AcademicAccreditation,
                    CommissionMode = university.CommissionMode,
                    TotalPrograms = activePrograms.Count,
                    TotalBTECPrograms = university.BtecPrograms?.Count(p => p.IsActive && p.IsApprovedByBtecAuthority) ?? 0,
                    TotalStudents = totalStudents,

                    // FIX: Safe Min/Max with Any() check
                    MinHourPrice = activePrograms.Any()
                        ? activePrograms.Min(p => p.HourPriceBase)
                        : 0,
                    MaxHourPrice = activePrograms.Any()
                        ? activePrograms.Max(p => p.HourPriceBase)
                        : 0,

                    // Map Regular Programs
                    RegularPrograms = activePrograms
                        .Select(up => new UniversityProgramCardViewModel
                        {
                            Id = up.Id,
                            UniversityId = up.UniversityId,
                            ProgramNameArabic = up.Program.NameArabic,
                            ProgramNameEnglish = up.Program.NameEnglish,
                            Degree = up.Program.Degree,
                            Language = up.Program.Language,
                            StudySystem = up.StudySystem,
                            DurationInYears = up.DurationInYears,
                            TotalCreditHours = up.Program.TotalCreditHours,
                            HourPriceBase = up.HourPriceBase,
                            RegistrationFeeFirstSemester = up.RegistrationFeeFirstSemester,
                            Capacity = up.Capacity,
                            IsActive = up.IsActive,
                            SemesterStartDate = up.SemesterStartDate,
                            // FIX: Safe Min with Any() check for EntryRequirements
                            MinGPA = up.EntryRequirements != null && up.EntryRequirements.Any()
                                ? up.EntryRequirements.Min(er => er.MinGPA)
                                : 0,
                            AcceptedPaths = up.EntryRequirements?
                                .Select(er => er.Path.ToString())
                                .Distinct()
                                .ToList() ?? new List<string>()
                        })
                        .OrderBy(p => p.ProgramNameEnglish)
                        .ToList(),

                    // Map BTEC Programs
                    BTECPrograms = university.BtecPrograms?
                        .Where(p => p.IsActive && p.IsApprovedByBtecAuthority)
                        .Select(bp => new BTECProgramCardViewModel
                        {
                            Id = bp.Id,
                            UniversityId = bp.UniversityId,
                            NameArabic = bp.NameArabic,
                            NameEnglish = bp.NameEnglish,
                            Level = bp.Level,
                            TechnicalField = bp.TechnicalField,
                            Language = bp.Language,
                            DurationInYears = bp.DurationInYears,
                            TotalCreditHours = bp.TotalCreditHours,
                            HourPriceBase = bp.HourPriceBase,
                            RegistrationFeeFirstSemester = bp.RegistrationFeeFirstSemester,
                            Capacity = bp.Capacity,
                            IsActive = bp.IsActive,
                            IsApprovedByBtecAuthority = bp.IsApprovedByBtecAuthority,
                            SemesterStartDate = bp.SemesterStartDate,
                            // FIX: Safe Min with Any() check for EntryRequirements
                            MinGPA = bp.EntryRequirements != null && bp.EntryRequirements.Any()
                                ? bp.EntryRequirements.Min(er => er.MinGPA)
                                : 0
                        })
                        .OrderBy(p => p.NameEnglish)
                        .ToList() ?? new List<BTECProgramCardViewModel>()
                };

                return View(viewModel);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading university details for ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the university details. Please try again.";
                return RedirectToAction(nameof(Universities));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}