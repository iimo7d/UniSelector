namespace Uni_Selector.ViewModels.Recommendations
{
    public class RecommendationDetailsViewModel
    {
        public int RecommendationId { get; set; }
        public string ProgramName { get; set; }
        public string ProgramNameArabic { get; set; }
        public string Description { get; set; }
        public string UniversityName { get; set; }
        public string UniversityLogo { get; set; }
        public string UniversityImage { get; set; }
        public string UniversityAddress { get; set; }
        public string UniversityPhone { get; set; }
        public string UniversityEmail { get; set; }
        public double Score { get; set; }
        public decimal EstimatedTotalCost { get; set; }
        public double? DistanceInKm { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public bool IsBtec { get; set; }
        public bool HasApplication { get; set; }
        public string ApplicationStatus { get; set; }
        public DateTime? ApplicationDate { get; set; }

        // Program Details
        public string Language { get; set; }
        public int TotalCreditHours { get; set; }
        public int DurationYears { get; set; }
        public decimal HourPriceBase { get; set; }
        public decimal RegistrationFeeFirstSemester { get; set; }
        public decimal RegistrationFeeRegularSemester { get; set; }
        public string StudySystem { get; set; }
        public DateTime SemesterStartDate { get; set; }
        public int Capacity { get; set; }
        public string Degree { get; set; }

        // Entry Requirements
        public double MinGPA { get; set; }
        public string RequiredPath { get; set; }

        // BTEC specific
        public string BtecLevel { get; set; }
        public string TechnicalField { get; set; }

        public string ScoreClass
        {
            get
            {
                if (Score >= 80) return "text-green-600";
                if (Score >= 60) return "text-blue-600";
                if (Score >= 40) return "text-yellow-600";
                return "text-gray-600";
            }
        }

        public string ScoreBadgeClass
        {
            get
            {
                if (Score >= 80) return "bg-green-100 text-green-800";
                if (Score >= 60) return "bg-blue-100 text-blue-800";
                if (Score >= 40) return "bg-yellow-100 text-yellow-800";
                return "bg-gray-100 text-gray-800";
            }
        }
    }
}
