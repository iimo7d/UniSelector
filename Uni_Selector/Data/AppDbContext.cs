using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        private static readonly DateTime SeedNow = new DateTime(2025, 12, 01, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime SemesterStart = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime StudentCreated1 = new DateTime(2025, 07, 01, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime StudentCreated2 = new DateTime(2025, 08, 10, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime StudentCreated3 = new DateTime(2025, 09, 05, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime StudentCreated4 = new DateTime(2025, 10, 01, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime AppBaseDate = new DateTime(2025, 11, 01, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime BtecApprovalDate = new DateTime(2025, 06, 01, 0, 0, 0, DateTimeKind.Utc);
        private const string AdminPasswordHash = "AQAAAAIAAYagAAAAEJVgs6mPsWwvEKY1wpShOzf2GxkLkDjwUhRoQjp2JEDkJh5qiC7nH06tMxyMwSb6GQ==";
        private const string BtexPasswordHash = "AQAAAAIAAYagAAAAEA+KJEMkwqIlD5VWv+IQSXKJZBlW+nEFN8m/5e5xYVjr/sioqXtgMtzc0jDLoBbXXw==";
        private const string RepPasswordHash = "AQAAAAIAAYagAAAAELNueszlDmvnXkCm6ndtewhFTyVPQ49pe04XrjMHrV18BV9lgUnRGvc6f4EZoCOfPA==";
        private const string StdPasswordHash = "AQAAAAIAAYagAAAAENtGxzgPJi6WhJ8QqfT9DN9fYamOfTr/cC8KY3o3dgQLviAZc3PEDD82Qkn0OoD4TQ==";



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Student> Students { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<UniversityRepresentative> UniversityRepresentatives { get; set; }
        public DbSet<ProgramEntity> Programs { get; set; }
        public DbSet<UniversityProgram> UniversityPrograms { get; set; }
        public DbSet<EntryRequirement> EntryRequirements { get; set; }
        public DbSet<BtecProgram> BtecPrograms { get; set; }
        public DbSet<BtecEntryRequirement> BtecEntryRequirements { get; set; }
        public DbSet<StudentApplication> StudentApplications { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<DiscountGrant> DiscountGrants { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<MonthlySettlement> MonthlySettlements { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes and relationships
            ModelConfiguration(modelBuilder);

            //// Seed data
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUniversities(modelBuilder);
            SeedUniRep(modelBuilder);
            SeedPrograms(modelBuilder);
            SeedUniversityPrograms(modelBuilder);
            SeedBtecPrograms(modelBuilder);
            SeedStudents(modelBuilder);
            SeedStudentApplications(modelBuilder);
        }

        private void ModelConfiguration(ModelBuilder modelBuilder)
        {
            //Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
            });

            //Configure Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => new { e.Province, e.City });
                entity.HasIndex(e => e.GPA);
                entity.HasIndex(e => e.Path);
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Configure University
            modelBuilder.Entity<University>(entity =>
            {
                entity.HasIndex(e => e.NameArabic);
                entity.HasIndex(e => e.NameEnglish);
                entity.HasIndex(e => new { e.Province, e.City });
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Type);
            });

            //Configure UniversityRepresentative
            modelBuilder.Entity<UniversityRepresentative>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.UniversityId);
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.University)
                    .WithMany(u => u.Representatives)
                    .HasForeignKey(e => e.UniversityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure ProgramEntity
            modelBuilder.Entity<ProgramEntity>(entity =>
            {
                entity.HasIndex(e => e.NameArabic);
                entity.HasIndex(e => e.NameEnglish);
                entity.HasIndex(e => e.Degree);
                entity.HasIndex(e => e.Language);
            });

            //Configure UniversityProgram
            modelBuilder.Entity<UniversityProgram>(entity =>
            {
                entity.HasIndex(e => e.UniversityId);
                entity.HasIndex(e => e.ProgramId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.UniversityId, e.ProgramId, e.StudySystem });

                entity.HasOne(e => e.University)
                    .WithMany(u => u.UniversityPrograms)
                    .HasForeignKey(e => e.UniversityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Program)
                    .WithMany(p => p.UniversityPrograms)
                    .HasForeignKey(e => e.ProgramId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Configure EntryRequirement
            modelBuilder.Entity<EntryRequirement>(entity =>
            {
                entity.HasIndex(e => e.UniversityProgramId);
                entity.HasIndex(e => e.Path);
                entity.HasIndex(e => e.MinGPA);

                entity.HasOne(e => e.UniversityProgram)
                    .WithMany(up => up.EntryRequirements)
                    .HasForeignKey(e => e.UniversityProgramId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure BtecProgram
            modelBuilder.Entity<BtecProgram>(entity =>
            {
                entity.HasIndex(e => e.UniversityId);
                entity.HasIndex(e => e.Level);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsApprovedByBtecAuthority);
                entity.HasIndex(e => e.TechnicalField);

                entity.HasOne(e => e.University)
                    .WithMany(u => u.BtecPrograms)
                    .HasForeignKey(e => e.UniversityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure BtecEntryRequirement
            modelBuilder.Entity<BtecEntryRequirement>(entity =>
            {
                entity.HasIndex(e => e.BtecProgramId);
                entity.HasIndex(e => e.MinGPA);

                entity.HasOne(e => e.BtecProgram)
                    .WithMany(bp => bp.EntryRequirements)
                    .HasForeignKey(e => e.BtecProgramId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure StudentApplication
            modelBuilder.Entity<StudentApplication>(entity =>
            {
                entity.HasIndex(e => e.StudentId);
                entity.HasIndex(e => e.UniversityProgramId);
                entity.HasIndex(e => e.BtecProgramId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ApplicationDate);
                entity.HasIndex(e => e.ApplicationNumber).IsUnique();

                entity.Property(e => e.HourDiscountPercent)
                     .HasPrecision(5, 2);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Applications)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UniversityProgram)
                    .WithMany()
                    .HasForeignKey(e => e.UniversityProgramId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BtecProgram)
                    .WithMany()
                    .HasForeignKey(e => e.BtecProgramId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.HourDiscountSetByUser)
                    .WithMany()
                    .HasForeignKey(e => e.HourDiscountSetByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Configure Recommendation
            modelBuilder.Entity<Recommendation>(entity =>
            {
                entity.HasIndex(e => e.StudentId);
                entity.HasIndex(e => e.UniversityProgramId);
                entity.HasIndex(e => e.BtecProgramId);
                entity.HasIndex(e => e.Score);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Recommendations)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UniversityProgram)
                    .WithMany()
                    .HasForeignKey(e => e.UniversityProgramId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BtecProgram)
                    .WithMany()
                    .HasForeignKey(e => e.BtecProgramId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Configure DiscountGrant
            modelBuilder.Entity<DiscountGrant>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.ApplicationId).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.GrantedAt);

                entity.HasOne(e => e.Application)
                    .WithOne(a => a.DiscountGrant)
                    .HasForeignKey<DiscountGrant>(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.University)
                    .WithMany()
                    .HasForeignKey(e => e.UniversityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RedeemedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.RedeemedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Configure Commission
            modelBuilder.Entity<Commission>(entity =>
            {
                entity.HasIndex(e => e.ApplicationId).IsUnique();
                entity.HasIndex(e => e.UniversityId);
                entity.HasIndex(e => e.Settled);
                entity.HasIndex(e => e.MonthlySettlementId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Application)
                    .WithOne(a => a.Commission)
                    .HasForeignKey<Commission>(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.University)
                    .WithMany()
                    .HasForeignKey(e => e.UniversityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MonthlySettlement)
                    .WithMany(ms => ms.Commissions)
                    .HasForeignKey(e => e.MonthlySettlementId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Configure MonthlySettlement
            modelBuilder.Entity<MonthlySettlement>(entity =>
            {
                entity.HasIndex(e => new { e.UniversityId, e.Year, e.Month }).IsUnique();
                entity.HasIndex(e => e.Closed);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.University)
                    .WithMany()
                    .HasForeignKey(e => e.UniversityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ClosedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ClosedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Configure Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Category);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "PlatformAdmin",
                    NormalizedName = "PLATFORMADMIN",
                    ConcurrencyStamp = "ROLE-PLATFORMADMIN-0001"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Student",
                    NormalizedName = "STUDENT",
                    ConcurrencyStamp = "ROLE-STUDENT-0001"
                },
                new IdentityRole
                {
                    Id = "3",
                    Name = "UniversityRep",
                    NormalizedName = "UNIVERSITYREP",
                    ConcurrencyStamp = "ROLE-UNIVERSITYREP-0001"
                },
                new IdentityRole
                {
                    Id = "4",
                    Name = "BtecAuthority",
                    NormalizedName = "BTECAUTHORITY",
                    ConcurrencyStamp = "ROLE-BTECAUTHORITY-0001"
                }
            );
        }

        private void SeedUsers(ModelBuilder modelBuilder)
        {
            var adminUser = new ApplicationUser
            {
                Id = "admin-001",
                UserName = "admin@uni-selector.jo",
                NormalizedUserName = "ADMIN@UNI-SELECTOR.JO",
                Email = "admin@uni-selector.jo",
                NormalizedEmail = "ADMIN@UNI-SELECTOR.JO",
                EmailConfirmed = true,
                FullName = "Platform Administrator",
                IsActive = true,
                CreatedAt = SeedNow,
                PasswordHash = AdminPasswordHash, //Pre-hashed password => Admin@123
                SecurityStamp = "ADMIN-SEC-0001",
                ConcurrencyStamp = "ADMIN-CONC-0001",
                PhoneNumber = "+962790000001"

            };

            var btecUser = new ApplicationUser
            {
                Id = "btec-001",
                UserName = "btec@education.gov.jo",
                NormalizedUserName = "BTEC@EDUCATION.GOV.JO",
                Email = "btec@education.gov.jo",
                NormalizedEmail = "BTEC@EDUCATION.GOV.JO",
                EmailConfirmed = true,
                FullName = "BTEC Authority Officer",
                IsActive = true,
                CreatedAt = SeedNow,
                SecurityStamp = "BTEC-SEC-0001",
                PasswordHash = BtexPasswordHash, //Pre-hashed password => Btec@123
                ConcurrencyStamp = "BTEC-CONC-0001",
                PhoneNumber = "+962790000001"

            };

            var germanRepUser = new ApplicationUser
            {
                Id = "rep-001",
                UserName = "rep@gju.edu.jo",
                NormalizedUserName = "REP@GJU.EDU.JO",
                Email = "rep@gju.edu.jo",
                NormalizedEmail = "REP@GJU.EDU.JO",
                EmailConfirmed = true,
                FullName = "Ahmad Al-Hassan",
                IsActive = true,
                CreatedAt = SeedNow,
                SecurityStamp = "REP-001-SEC-0001",
                PasswordHash = RepPasswordHash, //Pre-hashed password => Rep@1234
                ConcurrencyStamp = "REP-001-CONC-0001",
                PhoneNumber = "+962790000001"

            };

            var meuRepUser = new ApplicationUser
            {
                Id = "rep-002",
                UserName = "rep@meu.edu.jo",
                NormalizedUserName = "REP@MEU.EDU.JO",
                Email = "rep@meu.edu.jo",
                NormalizedEmail = "REP@MEU.EDU.JO",
                EmailConfirmed = true,
                FullName = "Sarah Al-Ahmad",
                IsActive = true,
                CreatedAt = SeedNow,
                SecurityStamp = "REP-002-SEC-0001",
                PasswordHash = RepPasswordHash,//Pre-hashed password => Rep@1234
                ConcurrencyStamp = "REP-002-CONC-0001",
                PhoneNumber = "+962790000001"

            };

            var student1User = new ApplicationUser
            {
                Id = "student-001",
                UserName = "mohammad.khaled@example.com",
                NormalizedUserName = "MOHAMMAD.KHALED@EXAMPLE.COM",
                Email = "mohammad.khaled@example.com",
                NormalizedEmail = "MOHAMMAD.KHALED@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Mohammad Khaled",
                IsActive = true,
                CreatedAt = SeedNow,
                SecurityStamp = "STUDENT-001-SEC-0001",
                PasswordHash = StdPasswordHash, // Pre-hashed password => Student@123
                ConcurrencyStamp = "STUDENT-001-CONC-0001",
                PhoneNumber = "+962790000001"
            };

            var student2User = new ApplicationUser
            {
                Id = "student-002",
                UserName = "fatima.hassan@example.com",
                NormalizedUserName = "FATIMA.HASSAN@EXAMPLE.COM",
                Email = "fatima.hassan@example.com",
                NormalizedEmail = "FATIMA.HASSAN@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Fatima Hassan",
                IsActive = true,
                CreatedAt = SeedNow,
                SecurityStamp = "STUDENT-002-SEC-0001",
                PasswordHash = StdPasswordHash, // Pre-hashed password => Student@123
                ConcurrencyStamp = "STUDENT-002-CONC-0001",
                PhoneNumber = "+962790000001"

            };

            var student3User = new ApplicationUser
            {
                Id = "student-003",
                UserName = "omar.salem@example.com",
                NormalizedUserName = "OMAR.SALEM@EXAMPLE.COM",
                Email = "omar.salem@example.com",
                NormalizedEmail = "OMAR.SALEM@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Omar Salem",
                IsActive = true,
                CreatedAt = SeedNow,
                SecurityStamp = "STUDENT-003-SEC-0001",
                PasswordHash = StdPasswordHash, // Pre-hashed password => Student@123
                ConcurrencyStamp = "STUDENT-003-CONC-0001",
                PhoneNumber = "+962790000001"

            };

            var student4User = new ApplicationUser
            {
                Id = "student-004",
                UserName = "leen.ibrahim@example.com",
                NormalizedUserName = "LEEN.IBRAHIM@EXAMPLE.COM",
                Email = "leen.ibrahim@example.com",
                NormalizedEmail = "LEEN.IBRAHIM@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Leen Ibrahim",
                IsActive = true,
                CreatedAt = SeedNow,
                SecurityStamp = "STUDENT-004-SEC-0001",
                PasswordHash = StdPasswordHash, // Pre-hashed password => Student@123
                ConcurrencyStamp = "STUDENT-004-CONC-0001",
                PhoneNumber = "+962790000001"

            };

            modelBuilder.Entity<ApplicationUser>().HasData(
                adminUser, btecUser, germanRepUser, meuRepUser,
                student1User, student2User, student3User, student4User
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "admin-001", RoleId = "1" },
                new IdentityUserRole<string> { UserId = "btec-001", RoleId = "4" },
                new IdentityUserRole<string> { UserId = "rep-001", RoleId = "3" },
                new IdentityUserRole<string> { UserId = "rep-002", RoleId = "3" },
                new IdentityUserRole<string> { UserId = "student-001", RoleId = "2" },
                new IdentityUserRole<string> { UserId = "student-002", RoleId = "2" },
                new IdentityUserRole<string> { UserId = "student-003", RoleId = "2" },
                new IdentityUserRole<string> { UserId = "student-004", RoleId = "2" }
            );
        }
        private void SeedUniversities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<University>().HasData(
                // German Jordanian University
                new University
                {
                    Id = 1,
                    NameArabic = "الجامعة الألمانية الأردنية",
                    NameEnglish = "German Jordanian University",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Madaba Street, Amman 11180, Jordan",
                    Latitude = 31.9539,
                    Longitude = 35.8666,
                    PhoneNumber = "+962-6-4294444",
                    Email = "info@gju.edu.jo",
                    OfficialWebsite = "https://www.gju.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Middle East University
                new University
                {
                    Id = 2,
                    NameArabic = "جامعة الشرق الأوسط",
                    NameEnglish = "Middle East University",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Airport Road, Amman 11831, Jordan",
                    Latitude = 31.9215,
                    Longitude = 35.9411,
                    PhoneNumber = "+962-6-4790222",
                    Email = "info@meu.edu.jo",
                    OfficialWebsite = "https://www.meu.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Isra University
                new University
                {
                    Id = 3,
                    NameArabic = "جامعة الإسراء",
                    NameEnglish = "Isra University",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "King Abdullah II Street, Amman 11622, Jordan",
                    Latitude = 31.9719,
                    Longitude = 35.8728,
                    PhoneNumber = "+962-6-4711710",
                    Email = "info@iu.edu.jo",
                    OfficialWebsite = "https://www.iu.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Zarqa University
                new University
                {
                    Id = 4,
                    NameArabic = "جامعة الزرقاء",
                    NameEnglish = "Zarqa University",
                    Type = UniversityType.Private,
                    Province = "Zarqa",
                    City = "Zarqa",
                    FullAddress = "Zarqa 13110, Jordan",
                    Latitude = 32.0833,
                    Longitude = 36.0833,
                    PhoneNumber = "+962-5-3821100",
                    Email = "info@zu.edu.jo",
                    OfficialWebsite = "https://www.zu.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Petra University
                new University
                {
                    Id = 5,
                    NameArabic = "جامعة البترا",
                    NameEnglish = "Petra University",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Airport Road, Amman 11196, Jordan",
                    Latitude = 31.9453,
                    Longitude = 35.9284,
                    PhoneNumber = "+962-6-5715546",
                    Email = "info@uop.edu.jo",
                    OfficialWebsite = "https://www.uop.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Al-Ahliyya Amman University
                new University
                {
                    Id = 6,
                    NameArabic = "جامعة عمان الأهلية",
                    NameEnglish = "Al-Ahliyya Amman University",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Al-Salt Highway, Amman 19328, Jordan",
                    Latitude = 32.0126,
                    Longitude = 35.8433,
                    PhoneNumber = "+962-6-5008000",
                    Email = "info@ammanu.edu.jo",
                    OfficialWebsite = "https://www.ammanu.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // NUCT (New University College of Technology)
                new University
                {
                    Id = 7,
                    NameArabic = "كلية الجامعة للتكنولوجيا الحديثة",
                    NameEnglish = "New University College of Technology",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Amman, Jordan",
                    Latitude = 31.9454,
                    Longitude = 35.9284,
                    PhoneNumber = "+962-6-5000000",
                    Email = "info@nuct.edu.jo",
                    OfficialWebsite = "https://www.nuct.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Al-Khawarizmi College
                new University
                {
                    Id = 8,
                    NameArabic = "كلية الخوارزمي الجامعية",
                    NameEnglish = "Al-Khawarizmi College",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Airport Road, Amman, Jordan",
                    Latitude = 31.9454,
                    Longitude = 35.9284,
                    PhoneNumber = "+962-6-4899450",
                    Email = "info@khawarizmi.edu.jo",
                    OfficialWebsite = "https://www.khawarizmi.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Arab Community College
                new University
                {
                    Id = 9,
                    NameArabic = "كلية المجتمع العربي",
                    NameEnglish = "Arab Community College",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Amman, Jordan",
                    Latitude = 31.9454,
                    Longitude = 35.9284,
                    PhoneNumber = "+962-6-5000000",
                    Email = "info@arabcollege.edu.jo",
                    OfficialWebsite = "https://www.arabcollege.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Applied Science Private University
                new University
                {
                    Id = 10,
                    NameArabic = "جامعة العلوم التطبيقية الخاصة",
                    NameEnglish = "Applied Science Private University",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Al-Arab Street, Amman 11931, Jordan",
                    Latitude = 31.9909,
                    Longitude = 35.8709,
                    PhoneNumber = "+962-6-5609999",
                    Email = "info@asu.edu.jo",
                    OfficialWebsite = "https://www.asu.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                },
                // Hussein Technical University (HTU)
                new University
                {
                    Id = 11,
                    NameArabic = "جامعة الحسين التقنية",
                    NameEnglish = "Hussein Technical University",
                    Type = UniversityType.Private,
                    Province = "Amman",
                    City = "Amman",
                    FullAddress = "Al-Hussein Bin Talal Street, Amman, Jordan",
                    Latitude = 31.9754,
                    Longitude = 35.8676,
                    PhoneNumber = "+962-6-5331000",
                    Email = "info@htu.edu.jo",
                    OfficialWebsite = "https://www.htu.edu.jo",
                    IsActive = true,
                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
                    CreatedAt = SeedNow
                }
            );
        }
        private void SeedUniRep(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UniversityRepresentative>().HasData(
                new UniversityRepresentative
                {
                    Id = 1,
                    UserId = "rep-001",
                    UniversityId = 1,
                    Position = "Admissions Officer",
                    IsActive = true,
                    CanManageFees = true,
                    CanManagePrograms = true,
                    CanViewApplications = true,
                    AssignedAt = SeedNow
                },
                new UniversityRepresentative
                {
                    Id = 2,
                    UserId = "rep-002",
                    UniversityId = 2,
                    Position = "Admissions Officer",
                    IsActive = true,
                    CanManageFees = true,
                    CanManagePrograms = true,
                    CanViewApplications = true,
                    AssignedAt = SeedNow
                }
            );
        }
        private void SeedPrograms(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProgramEntity>().HasData(
                // AI & Computer Science Programs
                new ProgramEntity
                {
                    Id = 1,
                    NameArabic = "الذكاء الاصطناعي",
                    NameEnglish = "Artificial Intelligence",
                    Description = "Bachelor's degree in Artificial Intelligence and Machine Learning",
                    Degree = Degree.Bachelor,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Computer Science",
                    TotalCreditHours = 132,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 2,
                    NameArabic = "التقنيات اللوجستية",
                    NameEnglish = "Logistics Technologies",
                    Description = "Bachelor's degree in Logistics and Supply Chain Management",
                    Degree = Degree.Bachelor,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Business Administration",
                    TotalCreditHours = 132,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 3,
                    NameArabic = "التصميم الداخلي",
                    NameEnglish = "Interior Design",
                    Description = "Bachelor's degree in Interior Design and Architecture",
                    Degree = Degree.Bachelor,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Arts and Design",
                    TotalCreditHours = 132,
                    CreatedAt = SeedNow
                },
                // Business Programs
                new ProgramEntity
                {
                    Id = 4,
                    NameArabic = "إدارة الأعمال الإلكترونية",
                    NameEnglish = "E-Business Management",
                    Description = "Diploma in Electronic Business Administration",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Business Administration",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 5,
                    NameArabic = "المحاسبة التقنية",
                    NameEnglish = "Technical Accounting",
                    Description = "Diploma in Technical Accounting",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Accounting",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                // Tourism & Hospitality
                new ProgramEntity
                {
                    Id = 6,
                    NameArabic = "إدارة الفنادق",
                    NameEnglish = "Hotel Management",
                    Description = "Diploma in Hotel and Hospitality Management",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Tourism and Hospitality",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 7,
                    NameArabic = "إدارة السياحة والفنادق",
                    NameEnglish = "Tourism and Hotel Management",
                    Description = "Diploma in Tourism and Hotel Management",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Tourism and Hospitality",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 8,
                    NameArabic = "فنون الطهي",
                    NameEnglish = "Culinary Arts",
                    Description = "Bachelor in Culinary Arts and Food Management",
                    Degree = Degree.Bachelor,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Hospitality",
                    TotalCreditHours = 132,
                    CreatedAt = SeedNow
                },
                // Engineering Programs
                new ProgramEntity
                {
                    Id = 9,
                    NameArabic = "تكنولوجيا إنشاء وصيانة المباني",
                    NameEnglish = "Building Construction and Maintenance Technology",
                    Description = "Diploma in Civil Engineering Technology",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Engineering",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 10,
                    NameArabic = "هندسة العمارة والتصميم الداخلي",
                    NameEnglish = "Architecture and Interior Design",
                    Description = "Diploma in Architecture and Interior Design",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Architecture",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 11,
                    NameArabic = "مساحة الطرق وحساب الكميات",
                    NameEnglish = "Road Surveying and Quantity Calculation",
                    Description = "Diploma in Road Surveying",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Engineering",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 12,
                    NameArabic = "تكنولوجيا الطاقة",
                    NameEnglish = "Energy Technology",
                    Description = "Diploma in Energy Technology",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Engineering",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                // IT Programs
                new ProgramEntity
                {
                    Id = 13,
                    NameArabic = "الاتصالات وشبكات الحاسوب",
                    NameEnglish = "Communications and Computer Networks",
                    Description = "Diploma in Computer Networks",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Information Technology",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 14,
                    NameArabic = "أمن المعلومات والشبكات",
                    NameEnglish = "Information Security and Networks",
                    Description = "Diploma in Cybersecurity",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Information Technology",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                // Medical Programs
                new ProgramEntity
                {
                    Id = 15,
                    NameArabic = "الصيدلة",
                    NameEnglish = "Pharmacy",
                    Description = "Diploma in Pharmacy",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Medical",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 16,
                    NameArabic = "التمريض المشارك",
                    NameEnglish = "Associate Nursing",
                    Description = "Diploma in Nursing",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Medical",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 17,
                    NameArabic = "طب الأسنان",
                    NameEnglish = "Dentistry",
                    Description = "Bachelor in Dentistry",
                    Degree = Degree.Bachelor,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Medical",
                    TotalCreditHours = 180,
                    CreatedAt = SeedNow
                },
                // Arts & Design
                new ProgramEntity
                {
                    Id = 18,
                    NameArabic = "التصميم الجرافيكي",
                    NameEnglish = "Graphic Design",
                    Description = "Diploma in Graphic Design",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Arts",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 19,
                    NameArabic = "فنون التصميم الداخلي والديكور",
                    NameEnglish = "Interior Design and Decoration Arts",
                    Description = "Diploma in Interior Decoration",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Arts",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 20,
                    NameArabic = "فنون السينما والتلفزيون",
                    NameEnglish = "Cinema and Television Arts",
                    Description = "Diploma in Film and TV Production",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Arts",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                },
                new ProgramEntity
                {
                    Id = 21,
                    NameArabic = "صناعة الأفلام",
                    NameEnglish = "Film Production",
                    Description = "Bachelor in Film Production",
                    Degree = Degree.Bachelor,
                    Language = LanguageCode.Both,
                    AcademicClassification = "Arts",
                    TotalCreditHours = 132,
                    CreatedAt = SeedNow
                },
                // Languages
                new ProgramEntity
                {
                    Id = 22,
                    NameArabic = "اللغة الإنجليزية التطبيقية",
                    NameEnglish = "Applied English Language",
                    Description = "Diploma in Applied English",
                    Degree = Degree.Diploma,
                    Language = LanguageCode.English,
                    AcademicClassification = "Languages",
                    TotalCreditHours = 72,
                    CreatedAt = SeedNow
                }
            );
        }

        private void SeedUniversityPrograms(ModelBuilder modelBuilder)
        {
            var semesterStart = SemesterStart;
            var programs = new List<UniversityProgram>
            {
                // German Jordanian University (ID: 1)
                new UniversityProgram
                {
                    Id = 1,
                    UniversityId = 1,
                    ProgramId = 2, // Logistics Technologies
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 110m,
                    RegistrationFeeFirstSemester = 557m,
                    RegistrationFeeRegularSemester = 230m,
                    Capacity = 50,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 2,
                    UniversityId = 1,
                    ProgramId = 1, // AI
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 110m,
                    RegistrationFeeFirstSemester = 557m,
                    RegistrationFeeRegularSemester = 230m,
                    Capacity = 50,
                    IsActive = true,
                    CreatedAt = SeedNow
                },

                // Middle East University (ID: 2)
                new UniversityProgram
                {
                    Id = 3,
                    UniversityId = 2,
                    ProgramId = 1, // AI
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 65m,
                    RegistrationFeeFirstSemester = 450m,
                    RegistrationFeeRegularSemester = 450m,
                    Capacity = 80,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 4,
                    UniversityId = 2,
                    ProgramId = 2, // Logistics
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 58.5m,
                    RegistrationFeeFirstSemester = 450m,
                    RegistrationFeeRegularSemester = 450m,
                    Capacity = 60,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 5,
                    UniversityId = 2,
                    ProgramId = 3, // Interior Design
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 65m,
                    RegistrationFeeFirstSemester = 450m,
                    RegistrationFeeRegularSemester = 450m,
                    Capacity = 40,
                    IsActive = true,
                    CreatedAt = SeedNow
                },

                // Isra University (ID: 3)
                new UniversityProgram
                {
                    Id = 6,
                    UniversityId = 3,
                    ProgramId = 1, // AI
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 45m,
                    RegistrationFeeFirstSemester = 350m,
                    RegistrationFeeRegularSemester = 350m,
                    Capacity = 70,
                    IsActive = true,
                    CreatedAt = SeedNow
                },

                // Zarqa University (ID: 4)
                new UniversityProgram
                {
                    Id = 7,
                    UniversityId = 4,
                    ProgramId = 1, // AI
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 50m,
                    RegistrationFeeFirstSemester = 350m,
                    RegistrationFeeRegularSemester = 350m,
                    Capacity = 60,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 8,
                    UniversityId = 4,
                    ProgramId = 2, // Logistics
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 44m,
                    RegistrationFeeFirstSemester = 350m,
                    RegistrationFeeRegularSemester = 350m,
                    Capacity = 50,
                    IsActive = true,
                    CreatedAt = SeedNow
                },

                // Petra University (ID: 5)
                new UniversityProgram
                {
                    Id = 9,
                    UniversityId = 5,
                    ProgramId = 2, // Logistics
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 45m,
                    RegistrationFeeFirstSemester = 370m,
                    RegistrationFeeRegularSemester = 370m,
                    Capacity = 50,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 10,
                    UniversityId = 5,
                    ProgramId = 1, // AI
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 52.5m,
                    RegistrationFeeFirstSemester = 370m,
                    RegistrationFeeRegularSemester = 370m,
                    Capacity = 60,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 11,
                    UniversityId = 5,
                    ProgramId = 3, // Interior Design
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 75m,
                    RegistrationFeeFirstSemester = 370m,
                    RegistrationFeeRegularSemester = 370m,
                    Capacity = 30,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 12,
                    UniversityId = 5,
                    ProgramId = 21, // Film Production
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 48.75m,
                    RegistrationFeeFirstSemester = 370m,
                    RegistrationFeeRegularSemester = 370m,
                    Capacity = 30,
                    IsActive = true,
                    CreatedAt = SeedNow
                },

                // Al-Ahliyya Amman University (ID: 6)
                new UniversityProgram
                {
                    Id = 13,
                    UniversityId = 6,
                    ProgramId = 17, // Dentistry
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 5,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 91m,
                    RegistrationFeeFirstSemester = 500m,
                    RegistrationFeeRegularSemester = 500m,
                    Capacity = 25,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 14,
                    UniversityId = 6,
                    ProgramId = 1, // AI
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 80m,
                    RegistrationFeeFirstSemester = 350m,
                    RegistrationFeeRegularSemester = 350m,
                    Capacity = 50,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 15,
                    UniversityId = 6,
                    ProgramId = 8, // Culinary Arts
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 70m,
                    RegistrationFeeFirstSemester = 350m,
                    RegistrationFeeRegularSemester = 350m,
                    Capacity = 30,
                    IsActive = true,
                    CreatedAt = SeedNow
                },

                // Applied Science University (ID: 10)
                new UniversityProgram
                {
                    Id = 16,
                    UniversityId = 10,
                    ProgramId = 1, // AI
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 4,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 70m,
                    RegistrationFeeFirstSemester = 450m,
                    RegistrationFeeRegularSemester = 450m,
                    Capacity = 60,
                    IsActive = true,
                    CreatedAt = SeedNow
                },

                // Al-Khawarizmi College Diploma Programs (ID: 8)
                new UniversityProgram
                {
                    Id = 17,
                    UniversityId = 8,
                    ProgramId = 4, // E-Business Management
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 35m,
                    RegistrationFeeFirstSemester = 200m,
                    RegistrationFeeRegularSemester = 150m,
                    Capacity = 40,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 18,
                    UniversityId = 8,
                    ProgramId = 5, // Technical Accounting
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 35m,
                    RegistrationFeeFirstSemester = 200m,
                    RegistrationFeeRegularSemester = 150m,
                    Capacity = 40,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 19,
                    UniversityId = 8,
                    ProgramId = 6, // Hotel Management
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 35m,
                    RegistrationFeeFirstSemester = 200m,
                    RegistrationFeeRegularSemester = 150m,
                    Capacity = 30,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 20,
                    UniversityId = 8,
                    ProgramId = 14, // Cybersecurity
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 35m,
                    RegistrationFeeFirstSemester = 200m,
                    RegistrationFeeRegularSemester = 150m,
                    Capacity = 35,
                    IsActive = true,
                    CreatedAt = SeedNow
                },
                new UniversityProgram
                {
                    Id = 21,
                    UniversityId = 8,
                    ProgramId = 18, // Graphic Design
                    StudySystem = StudySystem.Morning,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    HourPriceBase = 35m,
                    RegistrationFeeFirstSemester = 200m,
                    RegistrationFeeRegularSemester = 150m,
                    Capacity = 30,
                    IsActive = true,
                    CreatedAt = SeedNow
                }
            };

            modelBuilder.Entity<UniversityProgram>().HasData(programs);

            // Seed Entry Requirements
            var entryRequirements = new List<EntryRequirement>();
            int reqId = 1;

            foreach (var program in programs)
            {
                // Academic - Scientific track
                entryRequirements.Add(new EntryRequirement
                {
                    Id = reqId++,
                    UniversityProgramId = program.Id,
                    MinGPA = program.HourPriceBase > 80 ? 75 : program.HourPriceBase > 60 ? 70 : 65,
                    Path = PathType.Academic,
                    AcademicTrack = AcademicTrack.Scientific,
                    AllowNoTawjihi = false,
                    EffectiveFrom = SeedNow
                });

                // Academic - Literary track
                entryRequirements.Add(new EntryRequirement
                {
                    Id = reqId++,
                    UniversityProgramId = program.Id,
                    MinGPA = program.HourPriceBase > 80 ? 70 : program.HourPriceBase > 60 ? 65 : 60,
                    Path = PathType.Academic,
                    AcademicTrack = AcademicTrack.Literary,
                    AllowNoTawjihi = false,
                    EffectiveFrom = SeedNow
                });
            }

            modelBuilder.Entity<EntryRequirement>().HasData(entryRequirements);
        }

        private void SeedBtecPrograms(ModelBuilder modelBuilder)
        {
            var semesterStart = SemesterStart;

            modelBuilder.Entity<BtecProgram>().HasData(
                // NUCT BTEC Programs (ID: 7)
                new BtecProgram
                {
                    Id = 1,
                    UniversityId = 7,
                    NameArabic = "تقنيات الأعمال - BTEC",
                    NameEnglish = "Business Technologies - BTEC",
                    Description = "BTEC Level 5 in Business Technologies",
                    Level = BtecLevel.Level5,
                    TechnicalField = "Business Administration",
                    Language = LanguageCode.Both,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    TotalCreditHours = 120,
                    HourPriceBase = 33.15m,
                    RegistrationFeeFirstSemester = 300m,
                    RegistrationFeeRegularSemester = 300m,
                    Capacity = 40,
                    IsActive = true,
                    IsApprovedByBtecAuthority = true,
                    ApprovalDate = BtecApprovalDate,
                    CreatedAt = SeedNow
                },
                // HTU BTEC Programs (ID: 11)
                new BtecProgram
                {
                    Id = 2,
                    UniversityId = 11,
                    NameArabic = "الهندسة التقنية - BTEC",
                    NameEnglish = "Technical Engineering - BTEC",
                    Description = "BTEC Level 5 in Technical Engineering",
                    Level = BtecLevel.Level5,
                    TechnicalField = "Engineering",
                    Language = LanguageCode.Both,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    TotalCreditHours = 120,
                    HourPriceBase = 100m,
                    RegistrationFeeFirstSemester = 350m,
                    RegistrationFeeRegularSemester = 350m,
                    Capacity = 30,
                    IsActive = true,
                    IsApprovedByBtecAuthority = true,
                    ApprovalDate = BtecApprovalDate,
                    CreatedAt = SeedNow
                },
                // Arab Community College BTEC (ID: 9)
                new BtecProgram
                {
                    Id = 3,
                    UniversityId = 9,
                    NameArabic = "تكنولوجيا المعلومات - BTEC",
                    NameEnglish = "Information Technology - BTEC",
                    Description = "BTEC Level 5 in Information Technology",
                    Level = BtecLevel.Level5,
                    TechnicalField = "Information Technology",
                    Language = LanguageCode.Both,
                    DurationInYears = 2,
                    SemesterStartDate = semesterStart,
                    TotalCreditHours = 120,
                    HourPriceBase = 28m,
                    RegistrationFeeFirstSemester = 250m,
                    RegistrationFeeRegularSemester = 250m,
                    Capacity = 35,
                    IsActive = true,
                    IsApprovedByBtecAuthority = true,
                    ApprovalDate = BtecApprovalDate,
                    CreatedAt = SeedNow
                }
            );

            // Seed BTEC Entry Requirements
            modelBuilder.Entity<BtecEntryRequirement>().HasData(
                new BtecEntryRequirement
                {
                    Id = 1,
                    BtecProgramId = 1,
                    MinGPA = 60,
                    RequiresBtecL2 = false,
                    RequiresBtecL3 = false,
                    Notes = "Accepts Tawjihi graduates with minimum 60%",
                    EffectiveFrom = SeedNow
                },
                new BtecEntryRequirement
                {
                    Id = 2,
                    BtecProgramId = 2,
                    MinGPA = 65,
                    RequiresBtecL2 = false,
                    RequiresBtecL3 = false,
                    Notes = "Accepts Tawjihi graduates with minimum 65%",
                    EffectiveFrom = SeedNow
                },
                new BtecEntryRequirement
                {
                    Id = 3,
                    BtecProgramId = 3,
                    MinGPA = 60,
                    RequiresBtecL2 = false,
                    RequiresBtecL3 = false,
                    Notes = "Accepts Tawjihi graduates with minimum 60%",
                    EffectiveFrom = SeedNow
                }
            );
        }

        private void SeedStudents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasData(
                // Student 1 - Mohammad Khaled (High achiever in Scientific track)
                new Student
                {
                    Id = 1,
                    UserId = "student-001",

                    // Location
                    Province = "Amman",
                    City = "Amman",
                    Area = "Abdoun",
                    Latitude = 31.9539,
                    Longitude = 35.8666,

                    // Education
                    GPA = 85.5,
                    Path = PathType.Academic,
                    AcademicTrack = AcademicTrack.Scientific,
                    VocationalBranch = null,

                    // BTEC
                    BtecLevel2Completed = false,
                    BtecLevel3Completed = false,
                    BtecCertificateUrl = null,  // ✅ Correct property name

                    // Preferences
                    RegistrationBudget = 10000m,
                    DesiredMajors = "الذكاء الاصطناعي, أمن المعلومات",
                    PreferredCity = "Amman",
                    MaxDistanceKm = 30,
                    PreferredLanguage = LanguageCode.Both,

                    // Family Connection
                    HasFamilyConnection = false,
                    FamilyConnectionUniversityId = null,

                    // Timestamps
                    CreatedAt = StudentCreated1,  // ✅ Static field
                    UpdatedAt = null,

                    // Identity
                    NationalId = "1234567890",
                    SeatNumber = "A10001",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(2006, 03, 15),
                    Nationality = "Jordanian",

                    // Guardian
                    GuardianName = "Khaled Ahmad",
                    GuardianPhone = "0791111111",
                    GuardianRelation = "Father",

                    // Special Status
                    HasDisability = false,
                    DisabilityType = null,
                    IsOrphan = false,
                    IsEmployeeChild = false,
                    IsActive = true,
                    ProfileCompleted = true,
                    
                    
                    // ❌ REMOVED: IsActive (doesn't exist)
                    // ❌ REMOVED: ProfileCompleted (doesn't exist)
                },

                // Student 2 - Fatima Hassan (Literary track, budget conscious)
                new Student
                {
                    Id = 2,
                    UserId = "student-002",

                    // Location
                    Province = "Zarqa",
                    City = "Zarqa",
                    Area = "Al-Zarqa Al-Jadida",
                    Latitude = 32.0833,
                    Longitude = 36.0833,

                    // Education
                    GPA = 72.0,
                    Path = PathType.Academic,
                    AcademicTrack = AcademicTrack.Literary,
                    VocationalBranch = null,

                    // BTEC
                    BtecLevel2Completed = false,
                    BtecLevel3Completed = false,
                    BtecCertificateUrl = null,

                    // Preferences
                    RegistrationBudget = 6000m,
                    DesiredMajors = "التصميم الداخلي",
                    PreferredCity = "Amman",
                    MaxDistanceKm = 60,
                    PreferredLanguage = LanguageCode.Arabic,

                    // Family Connection
                    HasFamilyConnection = true,
                    FamilyConnectionUniversityId = 2,  // MEU

                    // Timestamps
                    CreatedAt = StudentCreated2,
                    UpdatedAt = null,

                    // Identity
                    NationalId = "2234567890",
                    SeatNumber = "A20002",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(2006, 07, 22),
                    Nationality = "Jordanian",

                    // Guardian
                    GuardianName = "Hassan Ali",
                    GuardianPhone = "0792222222",
                    GuardianRelation = "Father",

                    // Special Status
                    HasDisability = false,
                    DisabilityType = null,
                    IsOrphan = false,
                    IsEmployeeChild = false
                },

                // Student 3 - Omar Salem (BTEC pathway)
                new Student
                {
                    Id = 3,
                    UserId = "student-003",

                    // Location
                    Province = "Amman",
                    City = "Amman",
                    Area = "Marj Al-Hamam",
                    Latitude = 31.8794,
                    Longitude = 35.8414,

                    // Education
                    GPA = 68.0,
                    Path = PathType.BTEC,
                    AcademicTrack = null,
                    VocationalBranch = null,

                    // BTEC
                    BtecLevel2Completed = true,
                    BtecLevel3Completed = true,
                    BtecCertificateUrl = "/uploads/btec/omar_salem_btec_cert.pdf",  // ✅ Has certificate

                    // Preferences
                    RegistrationBudget = 5000m,
                    DesiredMajors = "BTEC",
                    PreferredCity = "Amman",
                    MaxDistanceKm = 80,
                    PreferredLanguage = LanguageCode.English,

                    // Family Connection
                    HasFamilyConnection = false,
                    FamilyConnectionUniversityId = null,

                    // Timestamps
                    CreatedAt = StudentCreated3,
                    UpdatedAt = null,

                    // Identity
                    NationalId = "3234567890",
                    SeatNumber = "B30003",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(2006, 11, 08),
                    Nationality = "Jordanian",

                    // Guardian
                    GuardianName = "Salem Omar",
                    GuardianPhone = "0793333333",
                    GuardianRelation = "Father",

                    // Special Status
                    HasDisability = false,
                    DisabilityType = null,
                    IsOrphan = false,
                    IsEmployeeChild = false
                },

                // Student 4 - Leen Ibrahim (Scientific track, interested in medical)
                new Student
                {
                    Id = 4,
                    UserId = "student-004",

                    // Location
                    Province = "Amman",
                    City = "Amman",
                    Area = "Shmeisani",
                    Latitude = 31.9636,
                    Longitude = 35.9094,

                    // Education
                    GPA = 92.5,
                    Path = PathType.Academic,
                    AcademicTrack = AcademicTrack.Scientific,
                    VocationalBranch = null,

                    // BTEC
                    BtecLevel2Completed = false,
                    BtecLevel3Completed = false,
                    BtecCertificateUrl = null,

                    // Preferences
                    RegistrationBudget = 15000m,
                    DesiredMajors = "طب الأسنان",
                    PreferredCity = "Amman",
                    MaxDistanceKm = 20,
                    PreferredLanguage = LanguageCode.Both,

                    // Family Connection
                    HasFamilyConnection = false,
                    FamilyConnectionUniversityId = null,

                    // Timestamps
                    CreatedAt = StudentCreated4,
                    UpdatedAt = null,

                    // Identity
                    NationalId = "4234567890",
                    SeatNumber = "A40004",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(2006, 01, 30),
                    Nationality = "Jordanian",

                    // Guardian
                    GuardianName = "Ibrahim Mahmoud",
                    GuardianPhone = "0794444444",
                    GuardianRelation = "Father",

                    // Special Status
                    HasDisability = false,
                    DisabilityType = null,
                    IsOrphan = false,
                    IsEmployeeChild = false
                }
            );
        }
        private void SeedStudentApplications(ModelBuilder modelBuilder)
        {
            // ثابت لتجنب PendingModelChangesWarning
            var appBaseDate = AppBaseDate;

            modelBuilder.Entity<StudentApplication>().HasData(
                // 1) Mohammad -> MEU AI (Approved) + خصم ساعات
                new StudentApplication
                {
                    Id = 1,
                    StudentId = 1,
                    UniversityProgramId = 3,
                    BtecProgramId = null,

                    Status = ApplicationStatus.Approved,
                    ApplicationDate = appBaseDate,
                    ApprovalDate = appBaseDate.AddDays(5),
                    UpdatedAt = appBaseDate.AddDays(5),

                    Notes = "Application approved. Waiting for student confirmation.",
                    PlannedFirstSemesterHours = 15,

                    HourDiscountPercent = 35m,               // ضمن 30-50
                    HourDiscountSetByUserId = "rep-002",     // لازم يكون موجود في AspNetUsers
                    HourDiscountSetAt = appBaseDate.AddDays(5),

                    ApplicationNumber = "APP-2025-001",
                    AdmissionNumber = "ADM-MEU-2025-001",
                    RejectionReason = null
                },

                // 2) Mohammad -> GJU AI (UnderReview)
                new StudentApplication
                {
                    Id = 2,
                    StudentId = 1,
                    UniversityProgramId = 2,
                    BtecProgramId = null,

                    Status = ApplicationStatus.UnderReview,
                    ApplicationDate = appBaseDate.AddDays(2),
                    ApprovalDate = null,
                    UpdatedAt = appBaseDate.AddDays(2),

                    Notes = "Under review by university representative.",
                    PlannedFirstSemesterHours = 15,

                    HourDiscountPercent = null,
                    HourDiscountSetByUserId = null,
                    HourDiscountSetAt = null,

                    ApplicationNumber = "APP-2025-002",
                    AdmissionNumber = null,
                    RejectionReason = null
                },

                // 3) Fatima -> MEU Interior Design (Pending)
                new StudentApplication
                {
                    Id = 3,
                    StudentId = 2,
                    UniversityProgramId = 5,
                    BtecProgramId = null,

                    Status = ApplicationStatus.Pending,
                    ApplicationDate = appBaseDate.AddDays(7),
                    ApprovalDate = null,
                    UpdatedAt = appBaseDate.AddDays(7),

                    Notes = "New application submitted.",
                    PlannedFirstSemesterHours = 12,

                    HourDiscountPercent = null,
                    HourDiscountSetByUserId = null,
                    HourDiscountSetAt = null,

                    ApplicationNumber = "APP-2025-003",
                    AdmissionNumber = null,
                    RejectionReason = null
                },

                // 4) Omar -> NUCT BTEC (Approved)
                new StudentApplication
                {
                    Id = 4,
                    StudentId = 3,
                    UniversityProgramId = null,
                    BtecProgramId = 1,

                    Status = ApplicationStatus.Approved,
                    ApplicationDate = appBaseDate.AddDays(10),
                    ApprovalDate = appBaseDate.AddDays(14),
                    UpdatedAt = appBaseDate.AddDays(14),

                    Notes = "BTEC application approved by NUCT.",
                    PlannedFirstSemesterHours = 18,

                    HourDiscountPercent = 30m,
                    HourDiscountSetByUserId = "rep-001",
                    HourDiscountSetAt = appBaseDate.AddDays(14),

                    ApplicationNumber = "APP-2025-004",
                    AdmissionNumber = "ADM-NUCT-2025-001",
                    RejectionReason = null
                },

                // 5) Leen -> AAU Dentistry (Enrolled)
                new StudentApplication
                {
                    Id = 5,
                    StudentId = 4,
                    UniversityProgramId = 13,
                    BtecProgramId = null,

                    Status = ApplicationStatus.Enrolled,
                    ApplicationDate = appBaseDate.AddDays(3),
                    ApprovalDate = appBaseDate.AddDays(6),
                    UpdatedAt = appBaseDate.AddDays(20),

                    Notes = "Student enrolled and paid first semester registration.",
                    PlannedFirstSemesterHours = 18,

                    HourDiscountPercent = 40m,
                    HourDiscountSetByUserId = "admin-001",
                    HourDiscountSetAt = appBaseDate.AddDays(6),

                    ApplicationNumber = "APP-2025-005",
                    AdmissionNumber = "ADM-AAU-2025-001",
                    RejectionReason = null
                },

                // 6) Fatima -> Al-Khawarizmi Graphic Design (UnderReview)
                new StudentApplication
                {
                    Id = 6,
                    StudentId = 2,
                    UniversityProgramId = 21,
                    BtecProgramId = null,

                    Status = ApplicationStatus.UnderReview,
                    ApplicationDate = appBaseDate.AddDays(12),
                    ApprovalDate = null,
                    UpdatedAt = appBaseDate.AddDays(13),

                    Notes = "Under review. Need additional documents.",
                    PlannedFirstSemesterHours = 15,

                    HourDiscountPercent = null,
                    HourDiscountSetByUserId = null,
                    HourDiscountSetAt = null,

                    ApplicationNumber = "APP-2025-006",
                    AdmissionNumber = null,
                    RejectionReason = null
                }
            );

            // =========================
            // Commission (One-to-One)
            // =========================
            modelBuilder.Entity<Commission>().HasData(
                new Commission
                {
                    Id = 1,
                    ApplicationId = 1,
                    UniversityId = 2,
                    Mode = CommissionMode.FirstSemesterRegistration2Percent,
                    Percentage = 2m,
                    BaseAmount = 450m,
                    RegistrationFeeUsed = 450m,
                    AmountEstimated = 9m,
                    Settled = false,
                    CreatedAt = appBaseDate.AddDays(5),
                    CalculatedAt = appBaseDate.AddDays(5),

                    HoursCountUsed = null,
                    HourPriceUsed = null,
                    DiscountPercentApplied = null,
                    MonthlySettlementId = null
                },
                new Commission
                {
                    Id = 2,
                    ApplicationId = 4,
                    UniversityId = 7,
                    Mode = CommissionMode.FirstSemesterRegistration2Percent,
                    Percentage = 2m,
                    BaseAmount = 300m,
                    RegistrationFeeUsed = 300m,
                    AmountEstimated = 6m,
                    Settled = false,
                    CreatedAt = appBaseDate.AddDays(14),
                    CalculatedAt = appBaseDate.AddDays(14),

                    HoursCountUsed = null,
                    HourPriceUsed = null,
                    DiscountPercentApplied = null,
                    MonthlySettlementId = null
                },
                new Commission
                {
                    Id = 3,
                    ApplicationId = 5,
                    UniversityId = 6,
                    Mode = CommissionMode.FirstSemesterRegistration2Percent,
                    Percentage = 2m,
                    BaseAmount = 500m,
                    RegistrationFeeUsed = 500m,
                    AmountEstimated = 10m,
                    Settled = false,
                    CreatedAt = appBaseDate.AddDays(6),
                    CalculatedAt = appBaseDate.AddDays(6),

                    HoursCountUsed = null,
                    HourPriceUsed = null,
                    DiscountPercentApplied = null,
                    MonthlySettlementId = null
                }
            );

            // =========================
            // DiscountGrant (One-to-One)
            // =========================
            modelBuilder.Entity<DiscountGrant>().HasData(
                new DiscountGrant
                {
                    Id = 1,
                    ApplicationId = 1,
                    Code = "DISC-MEU-2025-001",
                    UniversityId = 2,
                    Percentage = 5m,
                    AmountEstimated = 22.5m,
                    Status = DiscountStatus.Issued,
                    GrantedAt = appBaseDate.AddDays(5),

                    RedeemedAt = null,
                    RedeemedByUserId = null
                },
                new DiscountGrant
                {
                    Id = 2,
                    ApplicationId = 4,
                    Code = "DISC-NUCT-2025-001",
                    UniversityId = 7,
                    Percentage = 5m,
                    AmountEstimated = 15m,
                    Status = DiscountStatus.Redeemed,
                    GrantedAt = appBaseDate.AddDays(14),

                    RedeemedAt = appBaseDate.AddDays(20),
                    RedeemedByUserId = "rep-001"
                },
                new DiscountGrant
                {
                    Id = 3,
                    ApplicationId = 5,
                    Code = "DISC-AAU-2025-001",
                    UniversityId = 6,
                    Percentage = 5m,
                    AmountEstimated = 25m,
                    Status = DiscountStatus.Redeemed,
                    GrantedAt = appBaseDate.AddDays(6),

                    RedeemedAt = appBaseDate.AddDays(15),
                    RedeemedByUserId = "rep-002"
                }
            );
        }
    }
}










//protected override void OnModelCreating(ModelBuilder modelBuilder)
//{
//    base.OnModelCreating(modelBuilder);

//    //Configure ApplicationUser
//    modelBuilder.Entity<ApplicationUser>(entity =>
//    {
//        entity.HasIndex(e => e.Email).IsUnique();
//        entity.HasIndex(e => e.IsActive);
//        entity.HasIndex(e => e.CreatedAt);
//    });

//    //Configure Student
//    modelBuilder.Entity<Student>(entity =>
//    {
//        entity.HasIndex(e => e.UserId).IsUnique();
//        entity.HasIndex(e => new { e.Province, e.City });
//        entity.HasIndex(e => e.GPA);
//        entity.HasIndex(e => e.Path);
//        entity.HasIndex(e => e.IsActive);

//        entity.HasOne(e => e.User)
//            .WithMany()
//            .HasForeignKey(e => e.UserId)
//            .OnDelete(DeleteBehavior.Restrict);
//    });

//    //Configure University
//    modelBuilder.Entity<University>(entity =>
//    {
//        entity.HasIndex(e => e.NameArabic);
//        entity.HasIndex(e => e.NameEnglish);
//        entity.HasIndex(e => new { e.Province, e.City });
//        entity.HasIndex(e => e.IsActive);
//        entity.HasIndex(e => e.Type);
//    });

//    //Configure UniversityRepresentative
//    modelBuilder.Entity<UniversityRepresentative>(entity =>
//    {
//        entity.HasIndex(e => e.UserId);
//        entity.HasIndex(e => e.UniversityId);
//        entity.HasIndex(e => e.IsActive);

//        entity.HasOne(e => e.User)
//            .WithMany()
//            .HasForeignKey(e => e.UserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.University)
//            .WithMany(u => u.Representatives)
//            .HasForeignKey(e => e.UniversityId)
//            .OnDelete(DeleteBehavior.Cascade);
//    });

//    //Configure ProgramEntity
//    modelBuilder.Entity<ProgramEntity>(entity =>
//    {
//        entity.HasIndex(e => e.NameArabic);
//        entity.HasIndex(e => e.NameEnglish);
//        entity.HasIndex(e => e.Degree);
//        entity.HasIndex(e => e.Language);
//    });

//    //Configure UniversityProgram
//    modelBuilder.Entity<UniversityProgram>(entity =>
//    {
//        entity.HasIndex(e => e.UniversityId);
//        entity.HasIndex(e => e.ProgramId);
//        entity.HasIndex(e => e.IsActive);
//        entity.HasIndex(e => new { e.UniversityId, e.ProgramId, e.StudySystem });

//        entity.HasOne(e => e.University)
//            .WithMany(u => u.UniversityPrograms)
//            .HasForeignKey(e => e.UniversityId)
//            .OnDelete(DeleteBehavior.Cascade);

//        entity.HasOne(e => e.Program)
//            .WithMany(p => p.UniversityPrograms)
//            .HasForeignKey(e => e.ProgramId)
//            .OnDelete(DeleteBehavior.Restrict);
//    });

//    //Configure EntryRequirement
//    modelBuilder.Entity<EntryRequirement>(entity =>
//    {
//        entity.HasIndex(e => e.UniversityProgramId);
//        entity.HasIndex(e => e.Path);
//        entity.HasIndex(e => e.MinGPA);

//        entity.HasOne(e => e.UniversityProgram)
//            .WithMany(up => up.EntryRequirements)
//            .HasForeignKey(e => e.UniversityProgramId)
//            .OnDelete(DeleteBehavior.Cascade);
//    });

//    //Configure BtecProgram
//    modelBuilder.Entity<BtecProgram>(entity =>
//    {
//        entity.HasIndex(e => e.UniversityId);
//        entity.HasIndex(e => e.Level);
//        entity.HasIndex(e => e.IsActive);
//        entity.HasIndex(e => e.IsApprovedByBtecAuthority);
//        entity.HasIndex(e => e.TechnicalField);

//        entity.HasOne(e => e.University)
//            .WithMany(u => u.BtecPrograms)
//            .HasForeignKey(e => e.UniversityId)
//            .OnDelete(DeleteBehavior.Cascade);
//    });

//    //Configure BtecEntryRequirement
//    modelBuilder.Entity<BtecEntryRequirement>(entity =>
//    {
//        entity.HasIndex(e => e.BtecProgramId);
//        entity.HasIndex(e => e.MinGPA);

//        entity.HasOne(e => e.BtecProgram)
//            .WithMany(bp => bp.EntryRequirements)
//            .HasForeignKey(e => e.BtecProgramId)
//            .OnDelete(DeleteBehavior.Cascade);
//    });

//    //Configure StudentApplication
//    modelBuilder.Entity<StudentApplication>(entity =>
//    {
//        entity.HasIndex(e => e.StudentId);
//        entity.HasIndex(e => e.UniversityProgramId);
//        entity.HasIndex(e => e.BtecProgramId);
//        entity.HasIndex(e => e.Status);
//        entity.HasIndex(e => e.ApplicationDate);
//        entity.HasIndex(e => e.ApplicationNumber).IsUnique();

//        entity.HasOne(e => e.Student)
//            .WithMany(s => s.Applications)
//            .HasForeignKey(e => e.StudentId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.UniversityProgram)
//            .WithMany()
//            .HasForeignKey(e => e.UniversityProgramId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.BtecProgram)
//            .WithMany()
//            .HasForeignKey(e => e.BtecProgramId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.HourDiscountSetByUser)
//            .WithMany()
//            .HasForeignKey(e => e.HourDiscountSetByUserId)
//            .OnDelete(DeleteBehavior.Restrict);
//    });

//    //Configure Recommendation
//    modelBuilder.Entity<Recommendation>(entity =>
//    {
//        entity.HasIndex(e => e.StudentId);
//        entity.HasIndex(e => e.UniversityProgramId);
//        entity.HasIndex(e => e.BtecProgramId);
//        entity.HasIndex(e => e.Score);
//        entity.HasIndex(e => e.CreatedAt);

//        entity.HasOne(e => e.Student)
//            .WithMany(s => s.Recommendations)
//            .HasForeignKey(e => e.StudentId)
//            .OnDelete(DeleteBehavior.Cascade);

//        entity.HasOne(e => e.UniversityProgram)
//            .WithMany()
//            .HasForeignKey(e => e.UniversityProgramId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.BtecProgram)
//            .WithMany()
//            .HasForeignKey(e => e.BtecProgramId)
//            .OnDelete(DeleteBehavior.Restrict);
//    });

//    //Configure DiscountGrant
//    modelBuilder.Entity<DiscountGrant>(entity =>
//    {
//        entity.HasIndex(e => e.Code).IsUnique();
//        entity.HasIndex(e => e.ApplicationId).IsUnique();
//        entity.HasIndex(e => e.Status);
//        entity.HasIndex(e => e.GrantedAt);

//        entity.HasOne(e => e.Application)
//            .WithOne(a => a.DiscountGrant)
//            .HasForeignKey<DiscountGrant>(e => e.ApplicationId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.University)
//            .WithMany()
//            .HasForeignKey(e => e.UniversityId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.RedeemedByUser)
//            .WithMany()
//            .HasForeignKey(e => e.RedeemedByUserId)
//            .OnDelete(DeleteBehavior.Restrict);
//    });

//    //Configure Commission
//    modelBuilder.Entity<Commission>(entity =>
//    {
//        entity.HasIndex(e => e.ApplicationId).IsUnique();
//        entity.HasIndex(e => e.UniversityId);
//        entity.HasIndex(e => e.Settled);
//        entity.HasIndex(e => e.MonthlySettlementId);
//        entity.HasIndex(e => e.CreatedAt);

//        entity.HasOne(e => e.Application)
//            .WithOne(a => a.Commission)
//            .HasForeignKey<Commission>(e => e.ApplicationId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.University)
//            .WithMany()
//            .HasForeignKey(e => e.UniversityId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.MonthlySettlement)
//            .WithMany(ms => ms.Commissions)
//            .HasForeignKey(e => e.MonthlySettlementId)
//            .OnDelete(DeleteBehavior.Restrict);
//    });

//    //Configure MonthlySettlement
//    modelBuilder.Entity<MonthlySettlement>(entity =>
//    {
//        entity.HasIndex(e => new { e.UniversityId, e.Year, e.Month }).IsUnique();
//        entity.HasIndex(e => e.Closed);
//        entity.HasIndex(e => e.CreatedAt);

//        entity.HasOne(e => e.University)
//            .WithMany()
//            .HasForeignKey(e => e.UniversityId)
//            .OnDelete(DeleteBehavior.Restrict);

//        entity.HasOne(e => e.ClosedByUser)
//            .WithMany()
//            .HasForeignKey(e => e.ClosedByUserId)
//            .OnDelete(DeleteBehavior.Restrict);
//    });

//    //Configure Notification
//    modelBuilder.Entity<Notification>(entity =>
//    {
//        entity.HasIndex(e => e.UserId);
//        entity.HasIndex(e => e.IsRead);
//        entity.HasIndex(e => e.CreatedAt);
//        entity.HasIndex(e => e.Category);

//        entity.HasOne(e => e.User)
//            .WithMany(u => u.Notifications)
//            .HasForeignKey(e => e.UserId)
//            .OnDelete(DeleteBehavior.Cascade);
//    });

//    //Seed Data
//    SeedData(modelBuilder);
//}

//private void SeedData(ModelBuilder modelBuilder)
//{
//    var now = new DateTime(2025, 11, 1);

//    Seed Roles
//    var roles = new[]
//    {
//                new IdentityRole
//                {
//                    Id = "247845c3-87f8-46f1-a455-555c330c3c41",
//                    Name = Uni_Selector.Models.UserRoles.Student,
//                    NormalizedName = Uni_Selector.Models.UserRoles.Student.ToUpper(),
//                    ConcurrencyStamp = "d72e20f4-8245-4a69-ad2b-358a14cd4ce7"
//                },
//                new IdentityRole
//                {
//                    Id = "80f3af8c-17c5-456e-a4cc-15bb694e863b",
//                    Name = Uni_Selector.Models.UserRoles.UniversityRep,
//                    NormalizedName = Uni_Selector.Models.UserRoles.UniversityRep.ToUpper(),
//                    ConcurrencyStamp = "5b6cd610-3ead-4ec8-8d9d-2d4daf456312"
//                },
//                new IdentityRole
//                {
//                    Id = "dd348c1a-172c-41ce-81e7-d096a7596f93",
//                    Name = Uni_Selector.Models.UserRoles.PlatformAdmin,
//                    NormalizedName = Uni_Selector.Models.UserRoles.PlatformAdmin.ToUpper(),
//                    ConcurrencyStamp = "afa21429-070e-41b0-ad52-91e817a3beb2"
//                },
//                new IdentityRole
//                {
//                    Id = "912202c3-2b0f-4a73-94e5-6d85d64fa4db",
//                    Name = Uni_Selector.Models.UserRoles.BtecAuthority,
//                    NormalizedName = Uni_Selector.Models.UserRoles.BtecAuthority.ToUpper(),
//                    ConcurrencyStamp = "bf10b102-86d0-4caa-8e36-3434a5bb91c4"
//                }
//            };
//    modelBuilder.Entity<IdentityRole>().HasData(roles);

//    Seed Admin User
//   var hasher = new PasswordHasher<ApplicationUser>();
//    var adminUser = new ApplicationUser
//    {
//        Id = "bc7c5647-7196-414a-b94a-5c1a51ff492c",
//        UserName = "admin@uniselector.com",
//        NormalizedUserName = "ADMIN@UNISELECTOR.COM",
//        Email = "admin@uniselector.com",
//        NormalizedEmail = "ADMIN@UNISELECTOR.COM",
//        EmailConfirmed = true,
//        FullName = "Platform Administrator",
//        IsActive = true,
//        CreatedAt = now,
//        SecurityStamp = "19cfcb40-6c5b-4e83-8796-ab1abf7741f9",
//        ConcurrencyStamp = "4d33e595-96f6-48db-a638-33e59adba91b"
//    };
//    adminUser.PasswordHash = "AQAAAAIAAYagAAAAEPFUaq3Y9kfJmwtE4V8iv3DpjWSDEPZV/zgI8eRG5DA/AI4lc65U32nkR5GAysqXmA==";
//    modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

//    Assign Admin Role
//    modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
//    {
//        UserId = adminUser.Id,
//        RoleId = "dd348c1a-172c-41ce-81e7-d096a7596f93" // PlatformAdmin
//    });

//    Seed Sample Universities
//   var universities = new[]
//   {
//                new University
//                {
//                    Id = 1,
//                    NameArabic = "الجامعة الأردنية",
//                    NameEnglish = "University of Jordan",
//                    Type = UniversityType.Private,
//                    Province = "Amman",
//                    City = "Amman",
//                    FullAddress = "Queen Rania Street, Amman 11942, Jordan",
//                    Latitude = 31.9539,
//                    Longitude = 35.9106,
//                    PhoneNumber = "+96265355000",
//                    Email = "info@ju.edu.jo",
//                    OfficialWebsite = "https://www.ju.edu.jo",
//                    IsActive = true,
//                    CommissionMode = CommissionMode.FirstSemesterRegistration2Percent,
//                    CreatedAt = now
//                },
//                new University
//                {
//                    Id = 2,
//                    NameArabic = "الجامعة الهاشمية",
//                    NameEnglish = "The Hashemite University",
//                    Type = UniversityType.Private,
//                    Province = "Zarqa",
//                    City = "Zarqa",
//                    FullAddress = "P.O. Box 330127, Zarqa 13133, Jordan",
//                    Latitude = 32.1365,
//                    Longitude = 36.0022,
//                    PhoneNumber = "+96253903333",
//                    Email = "info@hu.edu.jo",
//                    OfficialWebsite = "https://www.hu.edu.jo",
//                    IsActive = true,
//                    CommissionMode = CommissionMode.ProgramTotalHours2Percent,
//                    CreatedAt = now
//                },
//                new University
//                {
//                    Id = 3,
//                    NameArabic = "جامعة العلوم التطبيقية",
//                    NameEnglish = "Applied Science Private University",
//                    Type = UniversityType.Private,
//                    Province = "Amman",
//                    City = "Amman",
//                    FullAddress = "Al-Arab Street, Shafa Badran, Amman 11931, Jordan",
//                    Latitude = 32.0182,
//                    Longitude = 35.8767,
//                    PhoneNumber = "+96264609999",
//                    Email = "info@asu.edu.jo",
//                    OfficialWebsite = "https://www.asu.edu.jo",
//                    IsActive = true,
//                    CommissionMode = CommissionMode.FirstSemesterRegPlusHours2Percent,
//                    CreatedAt = now
//                }
//           };
//    modelBuilder.Entity<University>().HasData(universities);

//    Seed Sample Programs
//   var programs = new[]
//   {
//                new ProgramEntity
//                {
//                    Id = 1,
//                    NameArabic = "هندسة البرمجيات",
//                    NameEnglish = "Software Engineering",
//                    Description = "Bachelor's degree in Software Engineering",
//                    Degree = Degree.Bachelor,
//                    Language = LanguageCode.English,
//                    AcademicClassification = "Engineering",
//                    TotalCreditHours = 132,
//                    CreatedAt = now
//                },
//                new ProgramEntity
//                {
//                    Id = 2,
//                    NameArabic = "علوم الحاسوب",
//                    NameEnglish = "Computer Science",
//                    Description = "Bachelor's degree in Computer Science",
//                    Degree = Degree.Bachelor,
//                    Language = LanguageCode.English,
//                    AcademicClassification = "Engineering",
//                    TotalCreditHours = 132,
//                    CreatedAt = now
//                },
//                new ProgramEntity
//                {
//                    Id = 3,
//                    NameArabic = "إدارة الأعمال",
//                    NameEnglish = "Business Administration",
//                    Description = "Bachelor's degree in Business Administration",
//                    Degree = Degree.Bachelor,
//                    Language = LanguageCode.Both,
//                    AcademicClassification = "Business",
//                    TotalCreditHours = 126,
//                    CreatedAt = now
//                },
//                new ProgramEntity
//                {
//                    Id = 4,
//                    NameArabic = "الطب البشري",
//                    NameEnglish = "Medicine",
//                    Description = "Medical Doctor degree",
//                    Degree = Degree.Bachelor,
//                    Language = LanguageCode.English,
//                    AcademicClassification = "Medical",
//                    TotalCreditHours = 240,
//                    CreatedAt = now
//                },
//                new ProgramEntity
//                {
//                    Id = 5,
//                    NameArabic = "القانون",
//                    NameEnglish = "Law",
//                    Description = "Bachelor's degree in Law",
//                    Degree = Degree.Bachelor,
//                    Language = LanguageCode.Arabic,
//                    AcademicClassification = "Law",
//                    TotalCreditHours = 132,
//                    CreatedAt = now
//                }
//           };
//    modelBuilder.Entity<ProgramEntity>().HasData(programs);

//    Seed University Programs
//   var universityPrograms = new[]
//   {
//                 University of Jordan Programs
//                new UniversityProgram
//                {
//                    Id = 1,
//                    UniversityId = 1,
//                    ProgramId = 1,
//                    StudySystem = StudySystem.Morning,
//                    DurationInYears = 4,
//                    SemesterStartDate = new DateTime(2024, 9, 1),
//                    HourPriceBase = 85m,
//                    RegistrationFeeFirstSemester = 500m,
//                    RegistrationFeeRegularSemester = 300m,
//                    Capacity = 100,
//                    IsActive = true,
//                    CreatedAt = now
//                },
//                new UniversityProgram
//                {
//                    Id = 2,
//                    UniversityId = 1,
//                    ProgramId = 2,
//                    StudySystem = StudySystem.Morning,
//                    DurationInYears = 4,
//                    SemesterStartDate = new DateTime(2024, 9, 1),
//                    HourPriceBase = 80m,
//                    RegistrationFeeFirstSemester = 500m,
//                    RegistrationFeeRegularSemester = 300m,
//                    Capacity = 120,
//                    IsActive = true,
//                    CreatedAt = now
//                },
//                 Hashemite University Programs
//                new UniversityProgram
//                {
//                    Id = 3,
//                    UniversityId = 2,
//                    ProgramId = 3,
//                    StudySystem = StudySystem.Morning,
//                    DurationInYears = 4,
//                    SemesterStartDate = new DateTime(2024, 9, 1),
//                    HourPriceBase = 70m,
//                    RegistrationFeeFirstSemester = 400m,
//                    RegistrationFeeRegularSemester = 250m,
//                    Capacity = 150,
//                    IsActive = true,
//                    CreatedAt = now
//                },
//                 Applied Science University Programs
//                new UniversityProgram
//                {
//                    Id = 4,
//                    UniversityId = 3,
//                    ProgramId = 1,
//                    StudySystem = StudySystem.Evening,
//                    DurationInYears = 4,
//                    SemesterStartDate = new DateTime(2024, 9, 1),
//                    HourPriceBase = 90m,
//                    RegistrationFeeFirstSemester = 600m,
//                    RegistrationFeeRegularSemester = 350m,
//                    Capacity = 80,
//                    IsActive = true,
//                    CreatedAt = now
//                },
//                new UniversityProgram
//                {
//                    Id = 5,
//                    UniversityId = 3,
//                    ProgramId = 5,
//                    StudySystem = StudySystem.Morning,
//                    DurationInYears = 4,
//                    SemesterStartDate = new DateTime(2024, 9, 1),
//                    HourPriceBase = 75m,
//                    RegistrationFeeFirstSemester = 450m,
//                    RegistrationFeeRegularSemester = 275m,
//                    Capacity = 100,
//                    IsActive = true,
//                    CreatedAt = now
//                }
//           };
//    modelBuilder.Entity<UniversityProgram>().HasData(universityPrograms);

//    Seed Entry Requirements
//   var entryRequirements = new[]
//   {
//                 Software Engineering - Scientific Track
//                new EntryRequirement
//                {
//                    Id = 1,
//                    UniversityProgramId = 1,
//                    MinGPA = 80.0,
//                    Path = PathType.Academic,
//                    AcademicTrack = AcademicTrack.Scientific,
//                    AllowNoTawjihi = false,
//                    EffectiveFrom = new DateTime(2024, 1, 1)
//                },
//                 Computer Science - Scientific Track
//                new EntryRequirement
//                {
//                    Id = 2,
//                    UniversityProgramId = 2,
//                    MinGPA = 75.0,
//                    Path = PathType.Academic,
//                    AcademicTrack = AcademicTrack.Scientific,
//                    AllowNoTawjihi = false,
//                    EffectiveFrom = new DateTime(2024, 1, 1)
//                },
//                 Business Administration - Literary Track
//                new EntryRequirement
//                {
//                    Id = 3,
//                    UniversityProgramId = 3,
//                    MinGPA = 70.0,
//                    Path = PathType.Academic,
//                    AcademicTrack = AcademicTrack.Literary,
//                    AllowNoTawjihi = false,
//                    EffectiveFrom = new DateTime(2024, 1, 1)
//                },
//                 Business Administration - Scientific Track
//                new EntryRequirement
//                {
//                    Id = 4,
//                    UniversityProgramId = 3,
//                    MinGPA = 68.0,
//                    Path = PathType.Academic,
//                    AcademicTrack = AcademicTrack.Scientific,
//                    AllowNoTawjihi = false,
//                    EffectiveFrom = new DateTime(2024, 1, 1)
//                },
//                 Software Engineering (Evening) - Scientific Track
//                new EntryRequirement
//                {
//                    Id = 5,
//                    UniversityProgramId = 4,
//                    MinGPA = 75.0,
//                    Path = PathType.Academic,
//                    AcademicTrack = AcademicTrack.Scientific,
//                    AllowNoTawjihi = false,
//                    EffectiveFrom = new DateTime(2024, 1, 1)
//                },
//                 Law - Literary Track
//                new EntryRequirement
//                {
//                    Id = 6,
//                    UniversityProgramId = 5,
//                    MinGPA = 72.0,
//                    Path = PathType.Academic,
//                    AcademicTrack = AcademicTrack.Literary,
//                    AllowNoTawjihi = false,
//                    EffectiveFrom = new DateTime(2024, 1, 1)
//                },
//                 Law - Sharia Track
//                new EntryRequirement
//                {
//                    Id = 7,
//                    UniversityProgramId = 5,
//                    MinGPA = 70.0,
//                    Path = PathType.Academic,
//                    AcademicTrack = AcademicTrack.Sharia,
//                    AllowNoTawjihi = false,
//                    EffectiveFrom = new DateTime(2024, 1, 1)
//                }
//           };
//    modelBuilder.Entity<EntryRequirement>().HasData(entryRequirements);
//}
