namespace Uni_Selector.ViewModels.Home
{
    public class AboutViewModel
    {
        public string PlatformName { get; set; } = "Uni Selector";
        public string Title { get; set; } = "Welcome to Jordan's #1 University Selection Platform";
        public string Description { get; set; }
        public List<FeatureItem> Features { get; set; } = new();
        public List<StatItem> Statistics { get; set; } = new();
        public TeamSection Team { get; set; } = new();
    }
}
