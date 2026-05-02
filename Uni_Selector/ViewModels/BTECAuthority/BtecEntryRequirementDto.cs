namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BtecEntryRequirementDto
    {
        public int Id { get; set; }
        public double MinGPA { get; set; }
        public bool RequiresBtecL2 { get; set; }
        public bool RequiresBtecL3 { get; set; }
        public string? Notes { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
