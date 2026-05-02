using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Uni_Selector.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", "ROLE-PLATFORMADMIN-0001", "PlatformAdmin", "PLATFORMADMIN" },
                    { "2", "ROLE-STUDENT-0001", "Student", "STUDENT" },
                    { "3", "ROLE-UNIVERSITYREP-0001", "UniversityRep", "UNIVERSITYREP" },
                    { "4", "ROLE-BTECAUTHORITY-0001", "BtecAuthority", "BTECAUTHORITY" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FullName", "IsActive", "LastLoginAt", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "admin-001", 0, "ADMIN-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@uni-selector.jo", true, "Platform Administrator", true, null, false, null, "ADMIN@UNI-SELECTOR.JO", "ADMIN@UNI-SELECTOR.JO", "AQAAAAIAAYagAAAAEJVgs6mPsWwvEKY1wpShOzf2GxkLkDjwUhRoQjp2JEDkJh5qiC7nH06tMxyMwSb6GQ==", null, false, "ADMIN-SEC-0001", false, "admin@uni-selector.jo" },
                    { "btec-001", 0, "BTEC-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "btec@education.gov.jo", true, "BTEC Authority Officer", true, null, false, null, "BTEC@EDUCATION.GOV.JO", "BTEC@EDUCATION.GOV.JO", "AQAAAAIAAYagAAAAEA+KJEMkwqIlD5VWv+IQSXKJZBlW+nEFN8m/5e5xYVjr/sioqXtgMtzc0jDLoBbXXw==", null, false, "BTEC-SEC-0001", false, "btec@education.gov.jo" },
                    { "rep-001", 0, "REP-001-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rep@gju.edu.jo", true, "Ahmad Al-Hassan", true, null, false, null, "REP@GJU.EDU.JO", "REP@GJU.EDU.JO", "AQAAAAIAAYagAAAAELNueszlDmvnXkCm6ndtewhFTyVPQ49pe04XrjMHrV18BV9lgUnRGvc6f4EZoCOfPA==", null, false, "REP-001-SEC-0001", false, "rep@gju.edu.jo" },
                    { "rep-002", 0, "REP-002-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rep@meu.edu.jo", true, "Sarah Al-Ahmad", true, null, false, null, "REP@MEU.EDU.JO", "REP@MEU.EDU.JO", "AQAAAAIAAYagAAAAELNueszlDmvnXkCm6ndtewhFTyVPQ49pe04XrjMHrV18BV9lgUnRGvc6f4EZoCOfPA==", null, false, "REP-002-SEC-0001", false, "rep@meu.edu.jo" },
                    { "student-001", 0, "STUDENT-001-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mohammad.khaled@example.com", true, "Mohammad Khaled", true, null, false, null, "MOHAMMAD.KHALED@EXAMPLE.COM", "MOHAMMAD.KHALED@EXAMPLE.COM", "AQAAAAIAAYagAAAAENtGxzgPJi6WhJ8QqfT9DN9fYamOfTr/cC8KY3o3dgQLviAZc3PEDD82Qkn0OoD4TQ==", null, false, "STUDENT-001-SEC-0001", false, "mohammad.khaled@example.com" },
                    { "student-002", 0, "STUDENT-002-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "fatima.hassan@example.com", true, "Fatima Hassan", true, null, false, null, "FATIMA.HASSAN@EXAMPLE.COM", "FATIMA.HASSAN@EXAMPLE.COM", "AQAAAAIAAYagAAAAENtGxzgPJi6WhJ8QqfT9DN9fYamOfTr/cC8KY3o3dgQLviAZc3PEDD82Qkn0OoD4TQ==", null, false, "STUDENT-002-SEC-0001", false, "fatima.hassan@example.com" },
                    { "student-003", 0, "STUDENT-003-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "omar.salem@example.com", true, "Omar Salem", true, null, false, null, "OMAR.SALEM@EXAMPLE.COM", "OMAR.SALEM@EXAMPLE.COM", "AQAAAAIAAYagAAAAENtGxzgPJi6WhJ8QqfT9DN9fYamOfTr/cC8KY3o3dgQLviAZc3PEDD82Qkn0OoD4TQ==", null, false, "STUDENT-003-SEC-0001", false, "omar.salem@example.com" },
                    { "student-004", 0, "STUDENT-004-CONC-0001", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "leen.ibrahim@example.com", true, "Leen Ibrahim", true, null, false, null, "LEEN.IBRAHIM@EXAMPLE.COM", "LEEN.IBRAHIM@EXAMPLE.COM", "AQAAAAIAAYagAAAAENtGxzgPJi6WhJ8QqfT9DN9fYamOfTr/cC8KY3o3dgQLviAZc3PEDD82Qkn0OoD4TQ==", null, false, "STUDENT-004-SEC-0001", false, "leen.ibrahim@example.com" }
                });

            migrationBuilder.InsertData(
                table: "Programs",
                columns: new[] { "Id", "AcademicClassification", "CreatedAt", "Degree", "Description", "Language", "NameArabic", "NameEnglish", "TotalCreditHours" },
                values: new object[,]
                {
                    { 1, "Computer Science", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bachelor's degree in Artificial Intelligence and Machine Learning", 3, "الذكاء الاصطناعي", "Artificial Intelligence", 132 },
                    { 2, "Business Administration", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bachelor's degree in Logistics and Supply Chain Management", 3, "التقنيات اللوجستية", "Logistics Technologies", 132 },
                    { 3, "Arts and Design", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bachelor's degree in Interior Design and Architecture", 3, "التصميم الداخلي", "Interior Design", 132 },
                    { 4, "Business Administration", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Electronic Business Administration", 3, "إدارة الأعمال الإلكترونية", "E-Business Management", 72 },
                    { 5, "Accounting", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Technical Accounting", 3, "المحاسبة التقنية", "Technical Accounting", 72 },
                    { 6, "Tourism and Hospitality", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Hotel and Hospitality Management", 3, "إدارة الفنادق", "Hotel Management", 72 },
                    { 7, "Tourism and Hospitality", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Tourism and Hotel Management", 3, "إدارة السياحة والفنادق", "Tourism and Hotel Management", 72 },
                    { 8, "Hospitality", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bachelor in Culinary Arts and Food Management", 3, "فنون الطهي", "Culinary Arts", 132 },
                    { 9, "Engineering", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Civil Engineering Technology", 3, "تكنولوجيا إنشاء وصيانة المباني", "Building Construction and Maintenance Technology", 72 },
                    { 10, "Architecture", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Architecture and Interior Design", 3, "هندسة العمارة والتصميم الداخلي", "Architecture and Interior Design", 72 },
                    { 11, "Engineering", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Road Surveying", 3, "مساحة الطرق وحساب الكميات", "Road Surveying and Quantity Calculation", 72 },
                    { 12, "Engineering", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Energy Technology", 3, "تكنولوجيا الطاقة", "Energy Technology", 72 },
                    { 13, "Information Technology", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Computer Networks", 3, "الاتصالات وشبكات الحاسوب", "Communications and Computer Networks", 72 },
                    { 14, "Information Technology", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Cybersecurity", 3, "أمن المعلومات والشبكات", "Information Security and Networks", 72 },
                    { 15, "Medical", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Pharmacy", 3, "الصيدلة", "Pharmacy", 72 },
                    { 16, "Medical", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Nursing", 3, "التمريض المشارك", "Associate Nursing", 72 },
                    { 17, "Medical", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bachelor in Dentistry", 3, "طب الأسنان", "Dentistry", 180 },
                    { 18, "Arts", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Graphic Design", 3, "التصميم الجرافيكي", "Graphic Design", 72 },
                    { 19, "Arts", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Interior Decoration", 3, "فنون التصميم الداخلي والديكور", "Interior Design and Decoration Arts", 72 },
                    { 20, "Arts", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Film and TV Production", 3, "فنون السينما والتلفزيون", "Cinema and Television Arts", 72 },
                    { 21, "Arts", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bachelor in Film Production", 3, "صناعة الأفلام", "Film Production", 132 },
                    { 22, "Languages", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Diploma in Applied English", 1, "اللغة الإنجليزية التطبيقية", "Applied English Language", 72 }
                });

            migrationBuilder.InsertData(
                table: "Universities",
                columns: new[] { "Id", "AcademicAccreditation", "City", "CommissionMode", "CreatedAt", "Email", "FullAddress", "ImagePath", "IsActive", "Latitude", "LogoPath", "Longitude", "NameArabic", "NameEnglish", "OfficialWebsite", "PhoneNumber", "Province", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@gju.edu.jo", "Madaba Street, Amman 11180, Jordan", null, true, 31.953900000000001, null, 35.866599999999998, "الجامعة الألمانية الأردنية", "German Jordanian University", "https://www.gju.edu.jo", "+962-6-4294444", "Amman", 1, null },
                    { 2, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@meu.edu.jo", "Airport Road, Amman 11831, Jordan", null, true, 31.921500000000002, null, 35.941099999999999, "جامعة الشرق الأوسط", "Middle East University", "https://www.meu.edu.jo", "+962-6-4790222", "Amman", 1, null },
                    { 3, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@iu.edu.jo", "King Abdullah II Street, Amman 11622, Jordan", null, true, 31.971900000000002, null, 35.872799999999998, "جامعة الإسراء", "Isra University", "https://www.iu.edu.jo", "+962-6-4711710", "Amman", 1, null },
                    { 4, null, "Zarqa", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@zu.edu.jo", "Zarqa 13110, Jordan", null, true, 32.083300000000001, null, 36.083300000000001, "جامعة الزرقاء", "Zarqa University", "https://www.zu.edu.jo", "+962-5-3821100", "Zarqa", 1, null },
                    { 5, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@uop.edu.jo", "Airport Road, Amman 11196, Jordan", null, true, 31.9453, null, 35.928400000000003, "جامعة البترا", "Petra University", "https://www.uop.edu.jo", "+962-6-5715546", "Amman", 1, null },
                    { 6, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@ammanu.edu.jo", "Al-Salt Highway, Amman 19328, Jordan", null, true, 32.012599999999999, null, 35.843299999999999, "جامعة عمان الأهلية", "Al-Ahliyya Amman University", "https://www.ammanu.edu.jo", "+962-6-5008000", "Amman", 1, null },
                    { 7, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@nuct.edu.jo", "Amman, Jordan", null, true, 31.945399999999999, null, 35.928400000000003, "كلية الجامعة للتكنولوجيا الحديثة", "New University College of Technology", "https://www.nuct.edu.jo", "+962-6-5000000", "Amman", 1, null },
                    { 8, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@khawarizmi.edu.jo", "Airport Road, Amman, Jordan", null, true, 31.945399999999999, null, 35.928400000000003, "كلية الخوارزمي الجامعية", "Al-Khawarizmi College", "https://www.khawarizmi.edu.jo", "+962-6-4899450", "Amman", 1, null },
                    { 9, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@arabcollege.edu.jo", "Amman, Jordan", null, true, 31.945399999999999, null, 35.928400000000003, "كلية المجتمع العربي", "Arab Community College", "https://www.arabcollege.edu.jo", "+962-6-5000000", "Amman", 1, null },
                    { 10, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@asu.edu.jo", "Al-Arab Street, Amman 11931, Jordan", null, true, 31.9909, null, 35.870899999999999, "جامعة العلوم التطبيقية الخاصة", "Applied Science Private University", "https://www.asu.edu.jo", "+962-6-5609999", "Amman", 1, null },
                    { 11, null, "Amman", 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@htu.edu.jo", "Al-Hussein Bin Talal Street, Amman, Jordan", null, true, 31.9754, null, 35.867600000000003, "جامعة الحسين التقنية", "Hussein Technical University", "https://www.htu.edu.jo", "+962-6-5331000", "Amman", 1, null }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "1", "admin-001" },
                    { "4", "btec-001" },
                    { "3", "rep-001" },
                    { "3", "rep-002" },
                    { "2", "student-001" },
                    { "2", "student-002" },
                    { "2", "student-003" },
                    { "2", "student-004" }
                });

            migrationBuilder.InsertData(
                table: "BtecPrograms",
                columns: new[] { "Id", "ApprovalDate", "ApprovalNotes", "Capacity", "CreatedAt", "Description", "DurationInYears", "HourPriceBase", "IsActive", "IsApprovedByBtecAuthority", "Language", "Level", "NameArabic", "NameEnglish", "RegistrationFeeFirstSemester", "RegistrationFeeRegularSemester", "SemesterStartDate", "TechnicalField", "TotalCreditHours", "UniversityId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 40, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BTEC Level 5 in Business Technologies", 2, 33.15m, true, true, 3, 5, "تقنيات الأعمال - BTEC", "Business Technologies - BTEC", 300m, 300m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Business Administration", 120, 7, null },
                    { 2, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 30, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BTEC Level 5 in Technical Engineering", 2, 100m, true, true, 3, 5, "الهندسة التقنية - BTEC", "Technical Engineering - BTEC", 350m, 350m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", 120, 11, null },
                    { 3, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 35, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BTEC Level 5 in Information Technology", 2, 28m, true, true, 3, 5, "تكنولوجيا المعلومات - BTEC", "Information Technology - BTEC", 250m, 250m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Information Technology", 120, 9, null }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "AcademicTrack", "Area", "BtecCertificateUrl", "BtecLevel2Completed", "BtecLevel3Completed", "City", "CreatedAt", "DateOfBirth", "DesiredMajors", "DisabilityType", "FamilyConnectionUniversityId", "GPA", "Gender", "GuardianName", "GuardianPhone", "GuardianRelation", "HasDisability", "HasFamilyConnection", "IsActive", "IsEmployeeChild", "IsOrphan", "Latitude", "Longitude", "MaxDistanceKm", "NationalId", "Nationality", "Path", "PreferredCity", "PreferredLanguage", "ProfileCompleted", "Province", "RegistrationBudget", "SeatNumber", "UpdatedAt", "UserId", "VocationalBranch" },
                values: new object[,]
                {
                    { 1, 1, "Abdoun", null, false, false, "Amman", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "الذكاء الاصطناعي, أمن المعلومات", null, null, 85.5, 1, "Khaled Ahmad", "0791111111", "Father", false, false, true, false, false, 31.953900000000001, 35.866599999999998, 30, "1234567890", "Jordanian", 1, "Amman", 3, false, "Amman", 10000m, "A10001", null, "student-001", null },
                    { 2, 2, "Al-Zarqa Al-Jadida", null, false, false, "Zarqa", new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "التصميم الداخلي", null, 2, 72.0, 2, "Hassan Ali", "0792222222", "Father", false, true, true, false, false, 32.083300000000001, 36.083300000000001, 60, "2234567890", "Jordanian", 1, "Amman", 2, false, "Zarqa", 6000m, "A20002", null, "student-002", null },
                    { 3, null, "Marj Al-Hamam", "/uploads/btec/omar_salem_btec_cert.pdf", true, true, "Amman", new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "BTEC", null, null, 68.0, 1, "Salem Omar", "0793333333", "Father", false, false, true, false, false, 31.8794, 35.8414, 80, "3234567890", "Jordanian", 3, "Amman", 1, false, "Amman", 5000m, "B30003", null, "student-003", null },
                    { 4, 1, "Shmeisani", null, false, false, "Amman", new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "طب الأسنان", null, null, 92.5, 2, "Ibrahim Mahmoud", "0794444444", "Father", false, false, true, false, false, 31.9636, 35.909399999999998, 20, "4234567890", "Jordanian", 1, "Amman", 3, false, "Amman", 15000m, "A40004", null, "student-004", null }
                });

            migrationBuilder.InsertData(
                table: "UniversityPrograms",
                columns: new[] { "Id", "Capacity", "CreatedAt", "DurationInYears", "HourPriceBase", "IsActive", "ProgramId", "RegistrationFeeFirstSemester", "RegistrationFeeRegularSemester", "SemesterStartDate", "StudySystem", "UniversityId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 50, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 110m, true, 2, 557m, 230m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, null },
                    { 2, 50, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 110m, true, 1, 557m, 230m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, null },
                    { 3, 80, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 65m, true, 1, 450m, 450m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2, null },
                    { 4, 60, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 58.5m, true, 2, 450m, 450m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2, null },
                    { 5, 40, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 65m, true, 3, 450m, 450m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2, null },
                    { 6, 70, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 45m, true, 1, 350m, 350m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3, null },
                    { 7, 60, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 50m, true, 1, 350m, 350m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, null },
                    { 8, 50, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 44m, true, 2, 350m, 350m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, null },
                    { 9, 50, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 45m, true, 2, 370m, 370m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5, null },
                    { 10, 60, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 52.5m, true, 1, 370m, 370m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5, null },
                    { 11, 30, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 75m, true, 3, 370m, 370m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5, null },
                    { 12, 30, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 48.75m, true, 21, 370m, 370m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5, null },
                    { 13, 25, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 91m, true, 17, 500m, 500m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 6, null },
                    { 14, 50, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 80m, true, 1, 350m, 350m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 6, null },
                    { 15, 30, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 70m, true, 8, 350m, 350m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 6, null },
                    { 16, 60, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 70m, true, 1, 450m, 450m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 10, null },
                    { 17, 40, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 35m, true, 4, 200m, 150m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8, null },
                    { 18, 40, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 35m, true, 5, 200m, 150m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8, null },
                    { 19, 30, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 35m, true, 6, 200m, 150m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8, null },
                    { 20, 35, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 35m, true, 14, 200m, 150m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8, null },
                    { 21, 30, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 35m, true, 18, 200m, 150m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8, null }
                });

            migrationBuilder.InsertData(
                table: "BtecEntryRequirements",
                columns: new[] { "Id", "BtecProgramId", "EffectiveFrom", "EffectiveTo", "MinGPA", "Notes", "RequiresBtecL2", "RequiresBtecL3" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, "Accepts Tawjihi graduates with minimum 60%", false, false },
                    { 2, 2, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, "Accepts Tawjihi graduates with minimum 65%", false, false },
                    { 3, 3, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, "Accepts Tawjihi graduates with minimum 60%", false, false }
                });

            migrationBuilder.InsertData(
                table: "EntryRequirements",
                columns: new[] { "Id", "AcademicTrack", "AllowNoTawjihi", "EffectiveFrom", "EffectiveTo", "MinGPA", "Notes", "Path", "UniversityProgramId", "VocationalBranch" },
                values: new object[,]
                {
                    { 1, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 75.0, null, 1, 1, null },
                    { 2, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 1, null },
                    { 3, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 75.0, null, 1, 2, null },
                    { 4, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 2, null },
                    { 5, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 3, null },
                    { 6, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 3, null },
                    { 7, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 4, null },
                    { 8, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 4, null },
                    { 9, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 5, null },
                    { 10, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 5, null },
                    { 11, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 6, null },
                    { 12, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 6, null },
                    { 13, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 7, null },
                    { 14, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 7, null },
                    { 15, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 8, null },
                    { 16, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 8, null },
                    { 17, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 9, null },
                    { 18, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 9, null },
                    { 19, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 10, null },
                    { 20, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 10, null },
                    { 21, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 11, null },
                    { 22, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 11, null },
                    { 23, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 12, null },
                    { 24, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 12, null },
                    { 25, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 75.0, null, 1, 13, null },
                    { 26, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 13, null },
                    { 27, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 14, null },
                    { 28, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 14, null },
                    { 29, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 15, null },
                    { 30, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 15, null },
                    { 31, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 70.0, null, 1, 16, null },
                    { 32, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 16, null },
                    { 33, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 17, null },
                    { 34, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 17, null },
                    { 35, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 18, null },
                    { 36, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 18, null },
                    { 37, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 19, null },
                    { 38, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 19, null },
                    { 39, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 20, null },
                    { 40, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 20, null },
                    { 41, 1, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 65.0, null, 1, 21, null },
                    { 42, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 60.0, null, 1, 21, null }
                });

            migrationBuilder.InsertData(
                table: "StudentApplications",
                columns: new[] { "Id", "AdmissionNumber", "ApplicationDate", "ApplicationNumber", "ApprovalDate", "BtecProgramId", "HourDiscountPercent", "HourDiscountSetAt", "HourDiscountSetByUserId", "Notes", "PlannedFirstSemesterHours", "RejectionReason", "Status", "StudentId", "UniversityProgramId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "ADM-MEU-2025-001", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "APP-2025-001", new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, 35m, new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Utc), "rep-002", "Application approved. Waiting for student confirmation.", 15, null, 3, 1, 3, new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, null, new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Utc), "APP-2025-002", null, null, null, null, null, "Under review by university representative.", 15, null, 2, 1, 2, new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, null, new DateTime(2025, 11, 8, 0, 0, 0, 0, DateTimeKind.Utc), "APP-2025-003", null, null, null, null, null, "New application submitted.", 12, null, 1, 2, 5, new DateTime(2025, 11, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "ADM-NUCT-2025-001", new DateTime(2025, 11, 11, 0, 0, 0, 0, DateTimeKind.Utc), "APP-2025-004", new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 30m, new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), "rep-001", "BTEC application approved by NUCT.", 18, null, 3, 3, null, new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "ADM-AAU-2025-001", new DateTime(2025, 11, 4, 0, 0, 0, 0, DateTimeKind.Utc), "APP-2025-005", new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, 40m, new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Utc), "admin-001", "Student enrolled and paid first semester registration.", 18, null, 5, 4, 13, new DateTime(2025, 11, 21, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, null, new DateTime(2025, 11, 13, 0, 0, 0, 0, DateTimeKind.Utc), "APP-2025-006", null, null, null, null, null, "Under review. Need additional documents.", 15, null, 2, 2, 21, new DateTime(2025, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Commissions",
                columns: new[] { "Id", "AmountEstimated", "ApplicationId", "BaseAmount", "CalculatedAt", "CreatedAt", "DiscountPercentApplied", "HourPriceUsed", "HoursCountUsed", "Mode", "MonthlySettlementId", "Percentage", "RegistrationFeeUsed", "Settled", "UniversityId" },
                values: new object[,]
                {
                    { 1, 9m, 1, 450m, new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, null, 2m, 450m, false, 2 },
                    { 2, 6m, 4, 300m, new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, null, 2m, 300m, false, 7 },
                    { 3, 10m, 5, 500m, new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, null, 2m, 500m, false, 6 }
                });

            migrationBuilder.InsertData(
                table: "DiscountGrants",
                columns: new[] { "Id", "AmountEstimated", "ApplicationId", "Code", "GrantedAt", "Percentage", "RedeemedAt", "RedeemedByUserId", "Status", "UniversityId" },
                values: new object[,]
                {
                    { 1, 22.5m, 1, "DISC-MEU-2025-001", new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Utc), 5m, null, null, 1, 2 },
                    { 2, 15m, 4, "DISC-NUCT-2025-001", new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), 5m, new DateTime(2025, 11, 21, 0, 0, 0, 0, DateTimeKind.Utc), "rep-001", 2, 7 },
                    { 3, 25m, 5, "DISC-AAU-2025-001", new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Utc), 5m, new DateTime(2025, 11, 16, 0, 0, 0, 0, DateTimeKind.Utc), "rep-002", 2, 6 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "admin-001" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "4", "btec-001" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "rep-001" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "rep-002" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2", "student-001" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2", "student-002" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2", "student-003" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2", "student-004" });

            migrationBuilder.DeleteData(
                table: "BtecEntryRequirements",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BtecEntryRequirements",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BtecEntryRequirements",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Commissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Commissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Commissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "DiscountGrants",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DiscountGrants",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DiscountGrants",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "EntryRequirements",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "StudentApplications",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "StudentApplications",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StudentApplications",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "btec-001");

            migrationBuilder.DeleteData(
                table: "BtecPrograms",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BtecPrograms",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StudentApplications",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "StudentApplications",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "StudentApplications",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "rep-001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "rep-002");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "student-002");

            migrationBuilder.DeleteData(
                table: "BtecPrograms",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UniversityPrograms",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "student-001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "student-003");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "student-004");

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
