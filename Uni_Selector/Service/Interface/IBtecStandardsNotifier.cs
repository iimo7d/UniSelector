using Uni_Selector.Models;
using Uni_Selector.ViewModels.BTECAuthority;

namespace Uni_Selector.Service.Interface
{
    public interface IBtecStandardsNotifier
    {
        Task SendStandardsUpdateNotificationsAsync(UpdateBTECStandardsViewModel model, ApplicationUser updatedBy);
    }
}
