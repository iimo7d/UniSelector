namespace Uni_Selector.ViewModels.Home
{
    public class FaqCategory
    {
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string CategoryColor { get; set; } = "primaryColor";
        public List<FaqItem> Questions { get; set; } = new();
    }
}
