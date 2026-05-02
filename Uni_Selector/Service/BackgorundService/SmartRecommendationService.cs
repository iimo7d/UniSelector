using Microsoft.EntityFrameworkCore;
using Uni_Selector.Data;
using Uni_Selector.Service.Interface;

namespace Uni_Selector.Service.BackgorundService
{
    public class SmartRecommendationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SmartRecommendationService> _logger;
        private readonly IRecommendationUpdateQueue _updateQueue;

        // Check for updates every 30 seconds
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

        public SmartRecommendationService(
            IServiceProvider serviceProvider,
            ILogger<SmartRecommendationService> logger,
            IRecommendationUpdateQueue updateQueue)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _updateQueue = updateQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Smart Recommendation Service started - monitoring for program updates...");

            // Wait 10 seconds after startup before first check
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check if there are pending regeneration requests
                    if (_updateQueue.HasPendingRegenerations())
                    {
                        await ProcessPendingRegenerationsAsync();
                    }

                    // Wait for the next check
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in Smart Recommendation Service");

                    // Wait 5 minutes before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("🛑 Smart Recommendation Service stopped.");
        }

        private async Task ProcessPendingRegenerationsAsync()
        {
            var (forAllStudents, studentIds, reason) = _updateQueue.DequeuePendingRegenerations();

            _logger.LogInformation($"📢 Processing regeneration request - Reason: {reason}");

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var recommendationService = scope.ServiceProvider.GetRequiredService<RecommendationService>();

                if (forAllStudents)
                {
                    await RegenerateForAllStudentsAsync(context, recommendationService, reason);
                }
                else if (studentIds.Any())
                {
                    await RegenerateForSpecificStudentsAsync(context, recommendationService, studentIds, reason);
                }
            }
        }

        private async Task RegenerateForAllStudentsAsync(
            AppDbContext context,
            RecommendationService recommendationService,
            string reason)
        {
            var students = await context.Students
                .Where(s => s.ProfileCompleted == true)
                .Select(s => new { s.Id, s.User.FullName })
                .ToListAsync();

            if (!students.Any())
            {
                _logger.LogInformation("ℹ️ No students with completed profiles found.");
                return;
            }

            _logger.LogInformation($"🔄 Regenerating recommendations for ALL {students.Count} students...");

            int successCount = 0;
            int failCount = 0;
            var startTime = DateTime.UtcNow;

            foreach (var student in students)
            {
                try
                {
                    var recommendations = await recommendationService.GenerateRecommendationsAsync(student.Id);

                    _logger.LogDebug($"✅ {student.FullName}: {recommendations.Count} recommendations");
                    successCount++;

                    // Small delay to avoid overwhelming the database
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Failed for student {student.Id} ({student.FullName})");
                    failCount++;
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                $"✨ Regeneration complete! Success: {successCount}, Failed: {failCount}, Duration: {duration.TotalSeconds:F1}s - Reason: {reason}");
        }

        private async Task RegenerateForSpecificStudentsAsync(
            AppDbContext context,
            RecommendationService recommendationService,
            List<int> studentIds,
            string reason)
        {
            var students = await context.Students
                .Where(s => studentIds.Contains(s.Id) && s.ProfileCompleted == true)
                .Select(s => new { s.Id, s.User.FullName })
                .ToListAsync();

            if (!students.Any())
            {
                _logger.LogInformation("ℹ️ No matching students found for regeneration.");
                return;
            }

            _logger.LogInformation($"🔄 Regenerating recommendations for {students.Count} specific students...");

            int successCount = 0;
            int failCount = 0;

            foreach (var student in students)
            {
                try
                {
                    var recommendations = await recommendationService.GenerateRecommendationsAsync(student.Id);
                    _logger.LogInformation($"✅ {student.FullName}: {recommendations.Count} recommendations - Reason: {reason}");
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Failed for student {student.Id} ({student.FullName})");
                    failCount++;
                }
            }

            _logger.LogInformation($"✨ Specific regeneration complete! Success: {successCount}, Failed: {failCount}");
        }
    }
}
