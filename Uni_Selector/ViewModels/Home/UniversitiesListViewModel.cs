namespace Uni_Selector.ViewModels.Home
{
    public class UniversitiesListViewModel
    {
        public List<UniversityCardViewModel> Universities { get; set; } = new();
        public int TotalUniversities { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
        public string SelectedCity { get; set; } = string.Empty;
        public List<string> AvailableCities { get; set; } = new();
    }
}
