namespace Uni_Selector.ViewModels.UniversityManagement
{
    public class BtecProgramInfoDto
    {
        public int Id { get; set; }
        public string NameEnglish { get; set; }
        public string NameArabic { get; set; }
        public string Level { get; set; }
        public string TechnicalField { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public bool IsApprovedByBtecAuthority { get; set; }
    }
}
