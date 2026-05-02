using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class LevelEnrollmentDto
    {
        public BtecLevel Level { get; set; }
        public int EnrolledCount { get; set; }
        public int CompletedCount { get; set; }
        public double CompletionRate { get; set; }
    }
}
