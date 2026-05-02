namespace Uni_Selector.ViewModels.Home
{
    public class FaqItem
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsExpanded { get; set; } = false;
    }
}
