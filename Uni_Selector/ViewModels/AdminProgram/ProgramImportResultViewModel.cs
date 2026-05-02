namespace Uni_Selector.ViewModels.AdminProgram
{
    public class ProgramImportResultViewModel
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int DuplicateCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<ProgramImportItemViewModel> ImportedPrograms { get; set; } = new();
        public List<ProgramImportItemViewModel> SkippedPrograms { get; set; } = new();
    }
}
