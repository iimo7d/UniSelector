namespace Uni_Selector.ViewModels.Home
{
    public class FaqViewModel
    {
        public string Title { get; set; } = "Frequently Asked Questions";
        public string Description { get; set; } = "Find answers to common questions about our platform";
        public List<FaqCategory> Categories { get; set; } = new();
        public int TotalQuestions { get; set; }
    }
}
