namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BTECStandardsViewModel
    {
        public List<StandardByLevelDto> Standards { get; set; } = new();
        public List<StandardByLevelDto> StandardsByLevel { get; set; } = new();
        public List<QualityRequirementDto> QualityRequirements { get; set; } = new();
        public List<AccreditationCriteriaDto> AccreditationCriteria { get; set; } = new();
        public DateTime? LastUpdated { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}
