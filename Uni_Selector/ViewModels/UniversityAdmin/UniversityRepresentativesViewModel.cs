using Microsoft.AspNetCore.Mvc.Rendering;

namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class UniversityRepresentativesViewModel
    {
        public UniversityBasicInfoDto University { get; set; } = new();
        public List<RepresentativeDetailDto> Representatives { get; set; } = new();
        public List<SelectListItem> AvailableUsers { get; set; } = new();
    }
}
