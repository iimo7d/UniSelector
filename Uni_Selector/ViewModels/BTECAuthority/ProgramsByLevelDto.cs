using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class ProgramsByLevelDto
    {
        public BtecLevel Level { get; set; }
        public string LevelName { get; set; }
        public int Count { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
    }
}
