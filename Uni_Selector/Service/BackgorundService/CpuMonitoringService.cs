using Uni_Selector.Helpers;

namespace Uni_Selector.Service.BackgorundService
{
    public class CpuMonitoringService : BackgroundService
    {
        private readonly ILogger<CpuMonitoringService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(5);

        public CpuMonitoringService(ILogger<CpuMonitoringService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CPU Monitoring Service started");

            // Initialize CPU monitoring
            CpuUsageHelper.Reset();

            // Wait a bit before first reading
            await Task.Delay(1000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Update CPU usage (this will be cached for quick access)
                    var cpuUsage = CpuUsageHelper.GetCurrentCpuUsage();

                    _logger.LogDebug("CPU Usage: {CpuUsage:F2}%", cpuUsage);

                    // Wait before next update
                    await Task.Delay(_updateInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating CPU usage");
                    await Task.Delay(_updateInterval, stoppingToken);
                }
            }

            _logger.LogInformation("CPU Monitoring Service stopped");
        }
    }
}
