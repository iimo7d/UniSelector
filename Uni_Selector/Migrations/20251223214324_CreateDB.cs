using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uni_Selector.Migrations
{
    /// <inheritdoc />
    public partial class CreateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameArabic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEnglish = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Degree = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    AcademicClassification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalCreditHours = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameArabic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEnglish = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    AcademicAccreditation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OfficialWebsite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CommissionMode = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    SmsSent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BtecPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    NameArabic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEnglish = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    TechnicalField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    DurationInYears = table.Column<int>(type: "int", nullable: false),
                    SemesterStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCreditHours = table.Column<int>(type: "int", nullable: false),
                    HourPriceBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationFeeFirstSemester = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationFeeRegularSemester = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsApprovedByBtecAuthority = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BtecPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BtecPrograms_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlySettlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TotalCommission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StudentCount = table.Column<int>(type: "int", nullable: false),
                    Closed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlySettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlySettlements_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlySettlements_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    GPA = table.Column<double>(type: "float", nullable: false),
                    Path = table.Column<int>(type: "int", nullable: false),
                    AcademicTrack = table.Column<int>(type: "int", nullable: true),
                    VocationalBranch = table.Column<int>(type: "int", nullable: true),
                    BtecLevel2Completed = table.Column<bool>(type: "bit", nullable: false),
                    BtecLevel3Completed = table.Column<bool>(type: "bit", nullable: false),
                    BtecCertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegistrationBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DesiredMajors = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreferredCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxDistanceKm = table.Column<int>(type: "int", nullable: false),
                    PreferredLanguage = table.Column<int>(type: "int", nullable: false),
                    HasFamilyConnection = table.Column<bool>(type: "bit", nullable: false),
                    FamilyConnectionUniversityId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianRelation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HasDisability = table.Column<bool>(type: "bit", nullable: false),
                    DisabilityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOrphan = table.Column<bool>(type: "bit", nullable: false),
                    IsEmployeeChild = table.Column<bool>(type: "bit", nullable: false),
                    ProfileCompleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Universities_FamilyConnectionUniversityId",
                        column: x => x.FamilyConnectionUniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UniversityPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    StudySystem = table.Column<int>(type: "int", nullable: false),
                    DurationInYears = table.Column<int>(type: "int", nullable: false),
                    SemesterStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourPriceBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationFeeFirstSemester = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationFeeRegularSemester = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityPrograms_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UniversityPrograms_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniversityRepresentatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CanManagePrograms = table.Column<bool>(type: "bit", nullable: false),
                    CanManageFees = table.Column<bool>(type: "bit", nullable: false),
                    CanViewApplications = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityRepresentatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityRepresentatives_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UniversityRepresentatives_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BtecEntryRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BtecProgramId = table.Column<int>(type: "int", nullable: false),
                    MinGPA = table.Column<double>(type: "float", nullable: false),
                    RequiresBtecL2 = table.Column<bool>(type: "bit", nullable: false),
                    RequiresBtecL3 = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BtecEntryRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BtecEntryRequirements_BtecPrograms_BtecProgramId",
                        column: x => x.BtecProgramId,
                        principalTable: "BtecPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniversityProgramId = table.Column<int>(type: "int", nullable: false),
                    MinGPA = table.Column<double>(type: "float", nullable: false),
                    Path = table.Column<int>(type: "int", nullable: false),
                    AcademicTrack = table.Column<int>(type: "int", nullable: true),
                    VocationalBranch = table.Column<int>(type: "int", nullable: true),
                    AllowNoTawjihi = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryRequirements_UniversityPrograms_UniversityProgramId",
                        column: x => x.UniversityProgramId,
                        principalTable: "UniversityPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    UniversityProgramId = table.Column<int>(type: "int", nullable: true),
                    BtecProgramId = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: false),
                    ReasonsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DistanceInKm = table.Column<double>(type: "float", nullable: true),
                    EstimatedTotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_BtecPrograms_BtecProgramId",
                        column: x => x.BtecProgramId,
                        principalTable: "BtecPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recommendations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recommendations_UniversityPrograms_UniversityProgramId",
                        column: x => x.UniversityProgramId,
                        principalTable: "UniversityPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    UniversityProgramId = table.Column<int>(type: "int", nullable: true),
                    BtecProgramId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HourDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    HourDiscountSetByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HourDiscountSetAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedFirstSemesterHours = table.Column<int>(type: "int", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AdmissionNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentApplications_AspNetUsers_HourDiscountSetByUserId",
                        column: x => x.HourDiscountSetByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentApplications_BtecPrograms_BtecProgramId",
                        column: x => x.BtecProgramId,
                        principalTable: "BtecPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentApplications_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentApplications_UniversityPrograms_UniversityProgramId",
                        column: x => x.UniversityProgramId,
                        principalTable: "UniversityPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Commissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HoursCountUsed = table.Column<int>(type: "int", nullable: true),
                    RegistrationFeeUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HourPriceUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountPercentApplied = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    AmountEstimated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Settled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonthlySettlementId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commissions_MonthlySettlements_MonthlySettlementId",
                        column: x => x.MonthlySettlementId,
                        principalTable: "MonthlySettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Commissions_StudentApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "StudentApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Commissions_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountGrants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AmountEstimated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RedeemedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountGrants_AspNetUsers_RedeemedByUserId",
                        column: x => x.RedeemedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountGrants_StudentApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "StudentApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountGrants_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreatedAt",
                table: "AspNetUsers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsActive",
                table: "AspNetUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BtecEntryRequirements_BtecProgramId",
                table: "BtecEntryRequirements",
                column: "BtecProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_BtecEntryRequirements_MinGPA",
                table: "BtecEntryRequirements",
                column: "MinGPA");

            migrationBuilder.CreateIndex(
                name: "IX_BtecPrograms_IsActive",
                table: "BtecPrograms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BtecPrograms_IsApprovedByBtecAuthority",
                table: "BtecPrograms",
                column: "IsApprovedByBtecAuthority");

            migrationBuilder.CreateIndex(
                name: "IX_BtecPrograms_Level",
                table: "BtecPrograms",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_BtecPrograms_TechnicalField",
                table: "BtecPrograms",
                column: "TechnicalField");

            migrationBuilder.CreateIndex(
                name: "IX_BtecPrograms_UniversityId",
                table: "BtecPrograms",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_ApplicationId",
                table: "Commissions",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_CreatedAt",
                table: "Commissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_MonthlySettlementId",
                table: "Commissions",
                column: "MonthlySettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_Settled",
                table: "Commissions",
                column: "Settled");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_UniversityId",
                table: "Commissions",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountGrants_ApplicationId",
                table: "DiscountGrants",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountGrants_Code",
                table: "DiscountGrants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountGrants_GrantedAt",
                table: "DiscountGrants",
                column: "GrantedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountGrants_RedeemedByUserId",
                table: "DiscountGrants",
                column: "RedeemedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountGrants_Status",
                table: "DiscountGrants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountGrants_UniversityId",
                table: "DiscountGrants",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryRequirements_MinGPA",
                table: "EntryRequirements",
                column: "MinGPA");

            migrationBuilder.CreateIndex(
                name: "IX_EntryRequirements_Path",
                table: "EntryRequirements",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_EntryRequirements_UniversityProgramId",
                table: "EntryRequirements",
                column: "UniversityProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySettlements_Closed",
                table: "MonthlySettlements",
                column: "Closed");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySettlements_ClosedByUserId",
                table: "MonthlySettlements",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySettlements_CreatedAt",
                table: "MonthlySettlements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySettlements_UniversityId_Year_Month",
                table: "MonthlySettlements",
                columns: new[] { "UniversityId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Category",
                table: "Notifications",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_Degree",
                table: "Programs",
                column: "Degree");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_Language",
                table: "Programs",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_NameArabic",
                table: "Programs",
                column: "NameArabic");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_NameEnglish",
                table: "Programs",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_BtecProgramId",
                table: "Recommendations",
                column: "BtecProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_CreatedAt",
                table: "Recommendations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_Score",
                table: "Recommendations",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_StudentId",
                table: "Recommendations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_UniversityProgramId",
                table: "Recommendations",
                column: "UniversityProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_ApplicationDate",
                table: "StudentApplications",
                column: "ApplicationDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_ApplicationNumber",
                table: "StudentApplications",
                column: "ApplicationNumber",
                unique: true,
                filter: "[ApplicationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_BtecProgramId",
                table: "StudentApplications",
                column: "BtecProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_HourDiscountSetByUserId",
                table: "StudentApplications",
                column: "HourDiscountSetByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_Status",
                table: "StudentApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_StudentId",
                table: "StudentApplications",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_UniversityProgramId",
                table: "StudentApplications",
                column: "UniversityProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_FamilyConnectionUniversityId",
                table: "Students",
                column: "FamilyConnectionUniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_GPA",
                table: "Students",
                column: "GPA");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsActive",
                table: "Students",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Path",
                table: "Students",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Province_City",
                table: "Students",
                columns: new[] { "Province", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Universities_IsActive",
                table: "Universities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_NameArabic",
                table: "Universities",
                column: "NameArabic");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_NameEnglish",
                table: "Universities",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Province_City",
                table: "Universities",
                columns: new[] { "Province", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Type",
                table: "Universities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityPrograms_IsActive",
                table: "UniversityPrograms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityPrograms_ProgramId",
                table: "UniversityPrograms",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityPrograms_UniversityId",
                table: "UniversityPrograms",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityPrograms_UniversityId_ProgramId_StudySystem",
                table: "UniversityPrograms",
                columns: new[] { "UniversityId", "ProgramId", "StudySystem" });

            migrationBuilder.CreateIndex(
                name: "IX_UniversityRepresentatives_IsActive",
                table: "UniversityRepresentatives",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityRepresentatives_UniversityId",
                table: "UniversityRepresentatives",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityRepresentatives_UserId",
                table: "UniversityRepresentatives",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BtecEntryRequirements");

            migrationBuilder.DropTable(
                name: "Commissions");

            migrationBuilder.DropTable(
                name: "DiscountGrants");

            migrationBuilder.DropTable(
                name: "EntryRequirements");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "UniversityRepresentatives");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "MonthlySettlements");

            migrationBuilder.DropTable(
                name: "StudentApplications");

            migrationBuilder.DropTable(
                name: "BtecPrograms");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "UniversityPrograms");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropTable(
                name: "Universities");
        }
    }
}
