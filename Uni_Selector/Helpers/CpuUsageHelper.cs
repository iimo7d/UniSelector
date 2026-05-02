using System.Diagnostics;
using Uni_Selector.ViewModels.SystemHealth;

namespace Uni_Selector.Helpers
{
    public class CpuUsageHelper
    {
        private static DateTime _lastCpuCheckTime = DateTime.UtcNow;
        private static TimeSpan _lastCpuTime = TimeSpan.Zero;
        private static double _lastCpuUsage = 0.0;
        private static readonly object _lockObject = new object();

       
        public static double GetCurrentCpuUsage()
        {
            lock (_lockObject)
            {
                try
                {
                    using var process = Process.GetCurrentProcess();

                    var currentTime = DateTime.UtcNow;
                    var currentCpuTime = process.TotalProcessorTime;

                    // Calculate time differences
                    var timeDiff = (currentTime - _lastCpuCheckTime).TotalMilliseconds;
                    var cpuDiff = (currentCpuTime - _lastCpuTime).TotalMilliseconds;

                    // Avoid division by zero
                    if (timeDiff == 0)
                    {
                        return _lastCpuUsage;
                    }

                    // Calculate CPU usage percentage
                    // CPU time is summed across all cores, so we divide by processor count
                    var cpuUsagePercent = (cpuDiff / (Environment.ProcessorCount * timeDiff)) * 100.0;

                    // Ensure the value is within valid range
                    cpuUsagePercent = Math.Max(0.0, Math.Min(100.0, cpuUsagePercent));

                    // Update last known values
                    _lastCpuCheckTime = currentTime;
                    _lastCpuTime = currentCpuTime;
                    _lastCpuUsage = cpuUsagePercent;

                    return cpuUsagePercent;
                }
                catch (Exception)
                {
                    // Return last known value on error
                    return _lastCpuUsage;
                }
            }
        }

        
        public static double GetSystemCpuUsage()
        {
            try
            {
                // This requires elevated permissions on some systems
                // So we use a try-catch approach
#if WINDOWS
                using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // First call always returns 0
                Thread.Sleep(100); // Wait a bit for accurate reading
                return cpuCounter.NextValue();
#else
                // For non-Windows systems, return application CPU usage
                return GetCurrentCpuUsage();
#endif
            }
            catch
            {
                // Fall back to application CPU usage
                return GetCurrentCpuUsage();
            }
        }

        public static async Task<double> GetAverageCpuUsageAsync(int durationMilliseconds = 1000)
        {
            var samples = new List<double>();
            var sampleCount = Math.Max(3, durationMilliseconds / 100); // At least 3 samples
            var delay = durationMilliseconds / sampleCount;

            using var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpuTime = process.TotalProcessorTime;

            await Task.Delay(durationMilliseconds);

            var endTime = DateTime.UtcNow;
            var endCpuTime = process.TotalProcessorTime;

            var timeDiff = (endTime - startTime).TotalMilliseconds;
            var cpuDiff = (endCpuTime - startCpuTime).TotalMilliseconds;

            if (timeDiff == 0) return 0.0;

            var avgCpuUsage = (cpuDiff / (Environment.ProcessorCount * timeDiff)) * 100.0;
            return Math.Max(0.0, Math.Min(100.0, avgCpuUsage));
        }

        
        public static void Reset()
        {
            lock (_lockObject)
            {
                _lastCpuCheckTime = DateTime.UtcNow;
                using var process = Process.GetCurrentProcess();
                _lastCpuTime = process.TotalProcessorTime;
                _lastCpuUsage = 0.0;
            }
        }

      
        public static CpuInfo GetCpuInfo()
        {
            using var process = Process.GetCurrentProcess();
            return new CpuInfo
            {
                ProcessorCount = Environment.ProcessorCount,
                CurrentUsagePercent = GetCurrentCpuUsage(),
                TotalProcessorTime = process.TotalProcessorTime,
                UserProcessorTime = process.UserProcessorTime,
                PrivilegedProcessorTime = process.PrivilegedProcessorTime
            };
        }
    }


    
}
