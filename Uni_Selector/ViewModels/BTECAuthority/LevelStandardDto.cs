using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class LevelStandardDto
    {
        public BtecLevel Level { get; set; }
        public double MinimumGPA { get; set; }
        public int MinimumCreditHours { get; set; }
        public int RecommendedDuration { get; set; }
        public bool RequiresPreviousLevel { get; set; }
        public string Description { get; set; }
        public List<string> KeyLearningOutcomes { get; set; } = new();
    }
}
