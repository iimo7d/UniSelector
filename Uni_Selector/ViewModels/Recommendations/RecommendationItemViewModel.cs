namespace Uni_Selector.ViewModels.Recommendations
{
    public class RecommendationItemViewModel
    {
        public int Id { get; set; }
        public string ProgramName { get; set; }
        public string ProgramNameArabic { get; set; }
        public string UniversityName { get; set; }
        public string UniversityLogo { get; set; }
        public double Score { get; set; }
        public decimal EstimatedCost { get; set; }
        public double? DistanceKm { get; set; }
        public string Language { get; set; }
        public bool IsBtec { get; set; }
        public bool IsViewed { get; set; }
        public bool HasApplication { get; set; }
        public string ApplicationStatus { get; set; }

        public string ScoreClass
        {
            get
            {
                if (Score >= 80) return "bg-green-500";
                if (Score >= 60) return "bg-blue-500";
                if (Score >= 40) return "bg-yellow-500";
                return "bg-gray-500";
            }
        }

        public string ScoreLabel
        {
            get
            {
                if (Score >= 80) return "Excellent Match";
                if (Score >= 60) return "Good Match";
                if (Score >= 40) return "Fair Match";
                return "Low Match";
            }
        }
    }
}
