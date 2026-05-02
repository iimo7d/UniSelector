namespace Uni_Selector.ViewModels.UniversityManagement
{
    public class UniversityProgramInfoDto
    {
        public int Id { get; set; }
        public string ProgramNameEnglish { get; set; }
        public string ProgramNameArabic { get; set; }
        public string Degree { get; set; }
        public int DurationInYears { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }
}
