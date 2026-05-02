

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uni_Selector.Data;
using Uni_Selector.Models;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.Service
{
    /// <summary>
    /// EF Core 9 Enhanced Recommendation Algorithm
    /// - Bulk delete old recommendations (ExecuteDeleteAsync)
    /// - Requirements: pick min GPA relevant to student's Path/Track/Branch + effective dates
    /// - Location: if lat/long exist -> real KM (Haversine). Otherwise -> proximity bands (PreferredCity/City/Province)
    /// - Scoring: smooth requirement + budget, capped bonus, better ranking
    /// - Optional diversity cap: limit results per University (works via DB mapping)
    /// </summary>
    public class RecommendationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecommendationService>? _logger;

        // You can move this to configuration later
        private const int DefaultFirstSemesterHours = 15;

        public RecommendationService(AppDbContext context, ILogger<RecommendationService>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Recommendation>> GenerateRecommendationsAsync(
            int studentId,
            int top = 50,
            int maxPerUniversity = 0, // 0 = disabled
            CancellationToken ct = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    var student = await _context.Students
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == studentId, ct);

                    if (student == null || student.ProfileCompleted != true || !student.IsActive)
                    {
                        await tx.RollbackAsync(ct);
                        return new List<Recommendation>();
                    }

                    // EF Core 9: fast bulk delete
                    await _context.Recommendations
                        .Where(r => r.StudentId == studentId)
                        .ExecuteDeleteAsync(ct);

                    var generated = student.Path == PathType.BTEC
                        ? await GenerateBtecRecommendationsAsync(student, ct)
                        : await GenerateRegularProgramRecommendationsAsync(student, ct);

                    generated = generated
                        .OrderByDescending(r => r.Score)
                        .ToList();

                    if (maxPerUniversity > 0)
                        generated = await ApplyDiversityCapAsync(generated, maxPerUniversity, ct);

                    generated = generated
                        .OrderByDescending(r => r.Score)
                        .Take(top)
                        .ToList();

                    if (generated.Count > 0)
                    {
                        _context.Recommendations.AddRange(generated);
                        await _context.SaveChangesAsync(ct);
                    }

                    await tx.CommitAsync(ct);
                    return generated;
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(ct);
                    _logger?.LogError(ex, "GenerateRecommendationsAsync failed for StudentId={StudentId}", studentId);
                    return new List<Recommendation>();
                }
            });
        }

        // ==========================================================
        // REGULAR PROGRAMS
        // ==========================================================
        private async Task<List<Recommendation>> GenerateRegularProgramRecommendationsAsync(Student student, CancellationToken ct)
        {
            var nowUtc = DateTime.UtcNow;
            var studentGpa = NormalizeGpa(student.GPA);

            var programs = await _context.UniversityPrograms
                .AsNoTracking()
                .Include(up => up.University)
                .Include(up => up.Program)
                .Include(up => up.EntryRequirements)
                .Where(up => up.IsActive && up.University.IsActive)
                .ToListAsync(ct);

            var result = new List<Recommendation>();

            foreach (var program in programs)
            {
                // ---- Requirements (min GPA relevant to the student)
                var minGpa = GetMinGpaForStudent(program, student, nowUtc);

                // Hard reject only if clearly not eligible (small tolerance)
                if (minGpa.HasValue && studentGpa < (minGpa.Value - 0.5))
                    continue;

                // ---- Location (real KM if possible, otherwise proximity bands)
                var (distanceKm, locationScore, band) = GetLocationSignals(student, program.University);

                // Apply MaxDistanceKm
                if (!PassDistanceConstraint(student, distanceKm, band))
                    continue;

                // ---- Cost
                var firstSemesterHours = DefaultFirstSemesterHours;
                var firstSemesterCost = program.RegistrationFeeFirstSemester + (program.HourPriceBase * firstSemesterHours);

                // ---- Score (0..100)
                var score = CalculateProgramScoreV2(
                    student: student,
                    program: program,
                    studentGpa: studentGpa,
                    minGpa: minGpa,
                    locationScore: locationScore,
                    firstSemesterCost: firstSemesterCost);

                // ---- UI cost (first semester estimate)
                var estimatedCost = CalculateEstimatedCost(program, firstSemesterHours);

                // ---- Reasons
                var reasons = GenerateRecommendationReasonsV2(
                    student: student,
                    studentGpa: studentGpa,
                    program: program,
                    minGpa: minGpa,
                    score: score,
                    firstSemesterCost: firstSemesterCost,
                    distanceKm: distanceKm,
                    band: band);

                result.Add(new Recommendation
                {
                    StudentId = student.Id,
                    UniversityProgramId = program.Id,
                    BtecProgramId = null,
                    Score = score,
                    DistanceInKm = distanceKm, // null now if lat/long not filled
                    EstimatedTotalCost = estimatedCost,
                    ReasonsJson = JsonSerializer.Serialize(reasons),
                    CreatedAt = DateTime.UtcNow,
                    IsViewed = false
                });
            }

            return result;
        }

        // ==========================================================
        // BTEC PROGRAMS
        // ==========================================================
        private async Task<List<Recommendation>> GenerateBtecRecommendationsAsync(Student student, CancellationToken ct)
        {
            var nowUtc = DateTime.UtcNow;
            var studentGpa = NormalizeGpa(student.GPA);

            var programs = await _context.BtecPrograms
                .AsNoTracking()
                .Include(bp => bp.University)
                .Include(bp => bp.EntryRequirements)
                .Where(bp => bp.IsActive && bp.University.IsActive && bp.IsApprovedByBtecAuthority)
                .ToListAsync(ct);

            var result = new List<Recommendation>();

            foreach (var program in programs)
            {
                var (eligible, minGpa) = EvaluateBtecEligibility(program, student, studentGpa, nowUtc);
                if (!eligible) continue;

                var (distanceKm, locationScore, band) = GetLocationSignals(student, program.University);

                if (!PassDistanceConstraint(student, distanceKm, band))
                    continue;

                var firstSemesterHours = DefaultFirstSemesterHours;
                var firstSemesterCost = program.RegistrationFeeFirstSemester + (program.HourPriceBase * firstSemesterHours);

                var score = CalculateBtecProgramScoreV2(
                    student: student,
                    program: program,
                    studentGpa: studentGpa,
                    minGpa: minGpa,
                    locationScore: locationScore,
                    firstSemesterCost: firstSemesterCost);

                var estimatedCost = CalculateBtecEstimatedCost(program, firstSemesterHours);

                var reasons = GenerateBtecRecommendationReasonsV2(
                    student: student,
                    studentGpa: studentGpa,
                    program: program,
                    minGpa: minGpa,
                    score: score,
                    firstSemesterCost: firstSemesterCost,
                    distanceKm: distanceKm,
                    band: band);

                result.Add(new Recommendation
                {
                    StudentId = student.Id,
                    UniversityProgramId = null,
                    BtecProgramId = program.Id,
                    Score = score,
                    DistanceInKm = distanceKm,
                    EstimatedTotalCost = estimatedCost,
                    ReasonsJson = JsonSerializer.Serialize(reasons),
                    CreatedAt = DateTime.UtcNow,
                    IsViewed = false
                });
            }

            return result;
        }

        // ==========================================================
        // REQUIREMENTS (REGULAR)
        // ==========================================================
        private static double? GetMinGpaForStudent(UniversityProgram program, Student student, DateTime nowUtc)
        {
            var reqs = program.EntryRequirements ?? new List<EntryRequirement>();
            if (!reqs.Any()) return null;

            // active by date
            reqs = reqs.Where(r =>
                    (!r.EffectiveFrom.HasValue || r.EffectiveFrom.Value <= nowUtc) &&
                    (!r.EffectiveTo.HasValue || r.EffectiveTo.Value >= nowUtc))
                .ToList();

            if (!reqs.Any()) return null;

            var pathReqs = reqs.Where(r => r.Path == student.Path).ToList();

            // If there are no requirements for student's path => don't block.
            if (!pathReqs.Any()) return null;

            // Tighten by track/branch if student has them
            if (student.Path == PathType.Academic && student.AcademicTrack.HasValue)
            {
                var filtered = pathReqs
                    .Where(r => !r.AcademicTrack.HasValue || r.AcademicTrack == student.AcademicTrack)
                    .ToList();
                if (filtered.Any()) pathReqs = filtered;
            }

            if (student.Path == PathType.Vocational && student.VocationalBranch.HasValue)
            {
                var filtered = pathReqs
                    .Where(r => !r.VocationalBranch.HasValue || r.VocationalBranch == student.VocationalBranch)
                    .ToList();
                if (filtered.Any()) pathReqs = filtered;
            }

            return pathReqs.Min(r => r.MinGPA);
        }

        // ==========================================================
        // REQUIREMENTS (BTEC)
        // ==========================================================
        private static (bool Eligible, double? MinGpa) EvaluateBtecEligibility(
            BtecProgram program, Student student, double studentGpa, DateTime nowUtc)
        {
            var reqs = program.EntryRequirements ?? new List<BtecEntryRequirement>();
            if (!reqs.Any())
                return (true, null);

            reqs = reqs.Where(r =>
                    (!r.EffectiveFrom.HasValue || r.EffectiveFrom.Value <= nowUtc) &&
                    (!r.EffectiveTo.HasValue || r.EffectiveTo.Value >= nowUtc))
                .ToList();

            if (!reqs.Any())
                return (true, null);

            var minGpa = reqs.Min(r => r.MinGPA);

            foreach (var req in reqs)
            {
                if (studentGpa < req.MinGPA) continue;
                if (req.RequiresBtecL2 && !student.BtecLevel2Completed) continue;
                if (req.RequiresBtecL3 && !student.BtecLevel3Completed) continue;

                return (true, minGpa);
            }

            return (false, minGpa);
        }

        // ==========================================================
        // LOCATION (NOW: BANDS, LATER: REAL KM)
        // ==========================================================
        private enum ProximityBand
        {
            PreferredCity,
            SameCity,
            SameProvince,
            Other,
            Unknown
        }

        private static (double? distanceKm, double locationScore, ProximityBand band) GetLocationSignals(Student student, University university)
        {
            // Future: if you fill lat/long later, it will auto-switch to real KM
            var distanceKm = CalculateDistanceKmIfAvailable(student, university);
            if (distanceKm.HasValue)
            {
                var max = student.MaxDistanceKm > 0 ? student.MaxDistanceKm : 80;
                var score = 100 - (distanceKm.Value / max) * 80; // 0km=100, max=20
                score = Math.Clamp(score, 20, 100);
                return (distanceKm, score, ProximityBand.Unknown);
            }

            // Current: proximity bands (no fake km)
            var band = GetProximityBand(student, university);
            var scoreFromBand = LocationScoreFromBand(band);
            return (null, scoreFromBand, band);
        }

        private static ProximityBand GetProximityBand(Student s, University u)
        {
            if (!string.IsNullOrWhiteSpace(s.PreferredCity) &&
                !string.IsNullOrWhiteSpace(u.City) &&
                u.City.Contains(s.PreferredCity, StringComparison.OrdinalIgnoreCase))
                return ProximityBand.PreferredCity;

            if (!string.IsNullOrWhiteSpace(s.City) &&
                !string.IsNullOrWhiteSpace(u.City) &&
                string.Equals(s.City, u.City, StringComparison.OrdinalIgnoreCase))
                return ProximityBand.SameCity;

            if (!string.IsNullOrWhiteSpace(s.Province) &&
                !string.IsNullOrWhiteSpace(u.Province) &&
                string.Equals(s.Province, u.Province, StringComparison.OrdinalIgnoreCase))
                return ProximityBand.SameProvince;

            if (string.IsNullOrWhiteSpace(s.City) && string.IsNullOrWhiteSpace(s.Province))
                return ProximityBand.Unknown;

            return ProximityBand.Other;
        }

        private static double LocationScoreFromBand(ProximityBand band) => band switch
        {
            ProximityBand.PreferredCity => 100,
            ProximityBand.SameCity => 95,
            ProximityBand.SameProvince => 70,
            ProximityBand.Other => 40,
            _ => 50
        };

        private static bool PassDistanceConstraint(Student s, double? distanceKm, ProximityBand band)
        {
            if (s.MaxDistanceKm <= 0) return true;

            // If we have real distance, enforce it
            if (distanceKm.HasValue)
                return distanceKm.Value <= s.MaxDistanceKm;

            // Fallback interpretation (no GPS):
            // <=20 => only same city / preferred city
            // <=60 => allow same province too
            if (s.MaxDistanceKm <= 20)
                return band is ProximityBand.SameCity or ProximityBand.PreferredCity;

            if (s.MaxDistanceKm <= 60)
                return band is ProximityBand.SameCity or ProximityBand.PreferredCity or ProximityBand.SameProvince;

            // >60 => allow all, ranking will handle
            return true;
        }

        private static double? CalculateDistanceKmIfAvailable(Student s, University u)
        {
            if (!s.Latitude.HasValue || !s.Longitude.HasValue || !u.Latitude.HasValue || !u.Longitude.HasValue)
                return null;

            return HaversineKm(s.Latitude.Value, s.Longitude.Value, u.Latitude.Value, u.Longitude.Value);
        }

        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double deg) => deg * (Math.PI / 180.0);

        // ==========================================================
        // SCORING (REGULAR)
        // ==========================================================
        private double CalculateProgramScoreV2(
            Student student,
            UniversityProgram program,
            double studentGpa,
            double? minGpa,
            double locationScore,
            decimal firstSemesterCost)
        {
            var reqScore = minGpa.HasValue ? RequirementScore(studentGpa, minGpa.Value) : 55;
            var budScore = BudgetScore(student, firstSemesterCost);
            var majScore = MajorScore(student, program);
            var langScore = LanguageScore(student, program);
            var bonus = BonusPoints(student, program.UniversityId); // 0..15

            var total =
                (reqScore * 0.35) +
                (budScore * 0.20) +
                (locationScore * 0.20) +
                (majScore * 0.15) +
                (langScore * 0.10) +
                bonus;

            return Math.Clamp(total, 0, 100);
        }

        private static double RequirementScore(double studentGpa, double minGpa)
        {
            // Smooth: -10 margin => 0, 0 margin => 40, +15 margin => 100
            var margin = studentGpa - minGpa;
            var normalized = (margin + 10) / 25.0;
            return Math.Clamp(normalized * 100.0, 0, 100);
        }

        private static double BudgetScore(Student student, decimal firstSemesterCost)
        {
            if (student.RegistrationBudget <= 0) return 50;

            var ratio = (double)(firstSemesterCost / student.RegistrationBudget);
            if (ratio <= 1.0) return 100;
            if (ratio <= 1.2) return 70;
            if (ratio <= 1.5) return 40;
            return 15;
        }

        private static double MajorScore(Student student, UniversityProgram program)
        {
            var desired = ParseDesiredMajors(student.DesiredMajors);
            if (desired.Count == 0) return 55;

            var nameEn = program.Program?.NameEnglish ?? "";
            var nameAr = program.Program?.NameArabic ?? "";
            var cls = program.Program?.AcademicClassification ?? "";

            foreach (var m in desired)
            {
                if (nameEn.Contains(m, StringComparison.OrdinalIgnoreCase) ||
                    nameAr.Contains(m, StringComparison.OrdinalIgnoreCase) ||
                    cls.Contains(m, StringComparison.OrdinalIgnoreCase))
                    return 100;
            }

            return 40;
        }

        private static double LanguageScore(Student student, UniversityProgram program)
        {
            var lang = program.Program?.Language;
            if (!lang.HasValue) return 50;

            if (lang.Value == student.PreferredLanguage) return 100;
            if (lang.Value == LanguageCode.Both) return 90;
            return 55;
        }

        private static double BonusPoints(Student student, int universityId)
        {
            double bonus = 0;
            if (student.HasFamilyConnection && student.FamilyConnectionUniversityId == universityId) bonus += 7;
            if (student.IsEmployeeChild) bonus += 3;
            if (student.IsOrphan) bonus += 3;
            if (student.HasDisability) bonus += 2;
            return Math.Clamp(bonus, 0, 15);
        }

        // ==========================================================
        // SCORING (BTEC)
        // ==========================================================
        private double CalculateBtecProgramScoreV2(
            Student student,
            BtecProgram program,
            double studentGpa,
            double? minGpa,
            double locationScore,
            decimal firstSemesterCost)
        {
            var reqScore = minGpa.HasValue ? RequirementScore(studentGpa, minGpa.Value) : 55;
            var budScore = BudgetScore(student, firstSemesterCost);
            var langScore = LanguageScore(student, program);

            var completionScore = 40
                + (student.BtecLevel2Completed ? 30 : 0)
                + (student.BtecLevel3Completed ? 30 : 0);
            completionScore = Math.Clamp(completionScore, 0, 100);

            var bonus = BonusPoints(student, program.UniversityId);

            var total =
                (reqScore * 0.25) +
                (completionScore * 0.25) +
                (budScore * 0.20) +
                (locationScore * 0.20) +
                (langScore * 0.10) +
                bonus;

            return Math.Clamp(total, 0, 100);
        }

        private static double LanguageScore(Student student, BtecProgram program)
        {
            if (program.Language == student.PreferredLanguage) return 100;
            if (program.Language == LanguageCode.Both) return 90;
            return 55;
        }

        // ==========================================================
        // COST (first semester estimate)
        // ==========================================================
        private static decimal CalculateEstimatedCost(UniversityProgram program, int firstSemesterHours)
        {
            var total = program.RegistrationFeeFirstSemester + (program.HourPriceBase * firstSemesterHours);
            var discount = total * 0.05m; // optional assumption
            return total - discount;
        }

        private static decimal CalculateBtecEstimatedCost(BtecProgram program, int firstSemesterHours)
        {
            var total = program.RegistrationFeeFirstSemester + (program.HourPriceBase * firstSemesterHours);
            var discount = total * 0.05m;
            return total - discount;
        }

        // ==========================================================
        // REASONS (REGULAR)
        // ==========================================================
        private static List<string> GenerateRecommendationReasonsV2(
            Student student,
            double studentGpa,
            UniversityProgram program,
            double? minGpa,
            double score,
            decimal firstSemesterCost,
            double? distanceKm,
            ProximityBand band)
        {
            var reasons = new List<string>();

            if (minGpa.HasValue)
            {


                var margin = studentGpa - minGpa.Value;
                if (margin >= 10) reasons.Add("Your GPA significantly exceeds the minimum requirement.");
                else if (margin >= 0) reasons.Add("You meet the GPA requirement.");
                else reasons.Add("GPA is slightly below the minimum requirement (lower priority).");
            }

            if (student.RegistrationBudget > 0)
            {
                if (firstSemesterCost <= student.RegistrationBudget) reasons.Add("Estimated first-semester cost fits within your budget.");
                else reasons.Add("Estimated first-semester cost is above your budget (lower priority).");
            }

            if (distanceKm.HasValue)
                reasons.Add($"Estimated distance is about {Math.Round(distanceKm.Value, 1)} km.");
            else
                reasons.Add(band switch
                {
                    ProximityBand.PreferredCity => "Located in your preferred city.",
                    ProximityBand.SameCity => "Located in your city.",
                    ProximityBand.SameProvince => "Located in your province.",
                    ProximityBand.Other => "Located outside your province.",
                    _ => "Location is based on city/province (coordinates not provided yet)."
                });

            var lang = program.Program?.Language;
            if (lang.HasValue)
            {
                if (lang.Value == student.PreferredLanguage) reasons.Add("Taught in your preferred language.");
                else if (lang.Value == LanguageCode.Both) reasons.Add("Available in both Arabic and English.");
            }

            if (student.HasFamilyConnection && student.FamilyConnectionUniversityId == program.UniversityId)
                reasons.Add("You have a family connection at this university.");

            var desired = ParseDesiredMajors(student.DesiredMajors);
            if (desired.Count > 0)
            {
                var nameEn = program.Program?.NameEnglish ?? "";
                var nameAr = program.Program?.NameArabic ?? "";
                foreach (var m in desired)
                {
                    if (nameEn.Contains(m, StringComparison.OrdinalIgnoreCase) ||
                        nameAr.Contains(m, StringComparison.OrdinalIgnoreCase))
                        reasons.Add($"Matches your desired major: {m}");
                }
            }

            if (score >= 85) reasons.Add("Highly recommended based on your profile.");
            else if (score >= 70) reasons.Add("Good match based on your profile.");

            return reasons.Distinct().ToList();
        }

        // ==========================================================
        // REASONS (BTEC)
        // ==========================================================
        private static List<string> GenerateBtecRecommendationReasonsV2(
            Student student,
            double studentGpa,
            BtecProgram program,
            double? minGpa,
            double score,
            decimal firstSemesterCost,
            double? distanceKm,
            ProximityBand band)
        {
            var reasons = new List<string>();

            if (minGpa.HasValue)
            {
                var margin = studentGpa - minGpa.Value;
                if (margin >= 10) reasons.Add("Your GPA is strong relative to BTEC entry requirements.");
                else if (margin >= 0) reasons.Add("You meet the BTEC GPA requirement.");
                else reasons.Add("GPA is slightly below the minimum requirement (lower priority).");
            }

            if (student.BtecLevel2Completed) reasons.Add("You have completed BTEC Level 2.");
            if (student.BtecLevel3Completed) reasons.Add("You have completed BTEC Level 3.");

            if (program.Language == student.PreferredLanguage) reasons.Add("Taught in your preferred language.");
            else if (program.Language == LanguageCode.Both) reasons.Add("Available in both Arabic and English.");

            if (student.RegistrationBudget > 0)
            {
                if (firstSemesterCost <= student.RegistrationBudget) reasons.Add("Estimated first-semester cost fits within your budget.");
                else reasons.Add("Estimated first-semester cost is above your budget (lower priority).");
            }

            if (distanceKm.HasValue)
                reasons.Add($"Estimated distance is about {Math.Round(distanceKm.Value, 1)} km.");
            else
                reasons.Add(band switch
                {
                    ProximityBand.PreferredCity => "Located in your preferred city.",
                    ProximityBand.SameCity => "Located in your city.",
                    ProximityBand.SameProvince => "Located in your province.",
                    ProximityBand.Other => "Located outside your province.",
                    _ => "Location is based on city/province (coordinates not provided yet)."
                });

            if (student.HasFamilyConnection && student.FamilyConnectionUniversityId == program.UniversityId)
                reasons.Add("You have a family connection at this university.");

            if (score >= 85) reasons.Add("Highly recommended based on your profile.");
            else if (score >= 70) reasons.Add("Good match based on your profile.");

            return reasons.Distinct().ToList();
        }

        // ==========================================================
        // DIVERSITY CAP (works in EF Core 9)
        // - limits how many recommendations per University
        // ==========================================================
        private async Task<List<Recommendation>> ApplyDiversityCapAsync(
            List<Recommendation> generated,
            int maxPerUniversity,
            CancellationToken ct)
        {
            if (maxPerUniversity <= 0) return generated;

            var upIds = generated
                .Where(r => r.UniversityProgramId.HasValue)
                .Select(r => r.UniversityProgramId!.Value)
                .Distinct()
                .ToList();

            var btecIds = generated
                .Where(r => r.BtecProgramId.HasValue)
                .Select(r => r.BtecProgramId!.Value)
                .Distinct()
                .ToList();

            var upUniMap = upIds.Count == 0
                ? new Dictionary<int, int>()
                : await _context.UniversityPrograms.AsNoTracking()
                    .Where(x => upIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.UniversityId })
                    .ToDictionaryAsync(x => x.Id, x => x.UniversityId, ct);

            var btecUniMap = btecIds.Count == 0
                ? new Dictionary<int, int>()
                : await _context.BtecPrograms.AsNoTracking()
                    .Where(x => btecIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.UniversityId })
                    .ToDictionaryAsync(x => x.Id, x => x.UniversityId, ct);

            var counts = new Dictionary<int, int>();
            var result = new List<Recommendation>(generated.Count);

            foreach (var r in generated.OrderByDescending(x => x.Score))
            {
                int? uniId = null;

                if (r.UniversityProgramId.HasValue && upUniMap.TryGetValue(r.UniversityProgramId.Value, out var u1))
                    uniId = u1;

                if (!uniId.HasValue && r.BtecProgramId.HasValue && btecUniMap.TryGetValue(r.BtecProgramId.Value, out var u2))
                    uniId = u2;

                // If we can't infer university, just keep it.
                if (!uniId.HasValue)
                {
                    result.Add(r);
                    continue;
                }

                counts.TryGetValue(uniId.Value, out var c);
                if (c >= maxPerUniversity) continue;

                counts[uniId.Value] = c + 1;
                result.Add(r);
            }

            return result;
        }

        // ==========================================================
        // HELPERS
        // ==========================================================
        private static double NormalizeGpa(double gpa)
        {
            // If some students store GPA on 4.0 scale, normalize to 0..100
            if (gpa <= 4.0) return gpa * 25.0;
            return gpa;
        }

        private static List<string> ParseDesiredMajors(string? desiredMajors)
        {
            if (string.IsNullOrWhiteSpace(desiredMajors))
                return new List<string>();

            return desiredMajors
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

