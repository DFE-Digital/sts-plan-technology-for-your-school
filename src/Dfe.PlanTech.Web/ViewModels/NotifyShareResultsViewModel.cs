using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class NotifyShareResultsViewModel
    {
        public required ActionViewModel ActionModel { get; set; }
        public required List<NotifySendResult> SendResults { get; set; }
    }
}
