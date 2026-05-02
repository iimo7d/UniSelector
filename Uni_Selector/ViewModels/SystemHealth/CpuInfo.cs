namespace Uni_Selector.ViewModels.SystemHealth
{
    public class CpuInfo
    {
        public int ProcessorCount { get; set; }
        public double CurrentUsagePercent { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }

        public string ProcessorCountText => $"{ProcessorCount} Core{(ProcessorCount > 1 ? "s" : "")}";
        public string TotalProcessorTimeText => $"{TotalProcessorTime.TotalHours:F2}h";
        public string UsageText => $"{CurrentUsagePercent:F1}%";
    }
}
