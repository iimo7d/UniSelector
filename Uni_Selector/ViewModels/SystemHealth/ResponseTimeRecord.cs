namespace Uni_Selector.ViewModels.SystemHealth
{
    public class ResponseTimeRecord
    {
        public DateTime Timestamp { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public double ResponseTimeMs { get; set; }
    }
}
