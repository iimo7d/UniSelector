namespace Uni_Selector.ViewModels.AdminCommission
{
    public class SettlementListItemViewModel
    {
        public int Id { get; set; }
        public string UniversityName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Period { get; set; }
        public decimal TotalCommission { get; set; }
        public int StudentCount { get; set; }
        public bool Closed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedByUserName { get; set; }

        public string GetPeriod()
        {
            return new DateTime(Year, Month, 1).ToString("MMMM yyyy");
        }
    }
}
