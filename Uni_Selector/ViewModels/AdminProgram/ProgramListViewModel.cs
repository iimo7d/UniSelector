using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminProgram
{
    public class ProgramListViewModel
    {
        public List<ProgramListItemViewModel> Programs { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPrograms { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalPrograms / PageSize);
        public string? SearchTerm { get; set; }
        public Degree? Degree { get; set; }
        public LanguageCode? Language { get; set; }

        public List<Degree> Degrees { get; set; } = new();
        public List<LanguageCode> Languages { get; set; } = new();
    }
}
