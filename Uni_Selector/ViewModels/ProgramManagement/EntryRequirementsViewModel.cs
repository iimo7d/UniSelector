namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class EntryRequirementsViewModel
    {
        public int ProgramId { get; set; }
        public string ProgramNameArabic { get; set; }
        public string ProgramNameEnglish { get; set; }
        public bool CanManagePrograms { get; set; }
        public List<EntryRequirementDto> EntryRequirements { get; set; } = new();
    }
}
