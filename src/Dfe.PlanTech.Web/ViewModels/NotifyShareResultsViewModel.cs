using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class NotifyShareResultsViewModel
{
    public required ActionViewModel ActionModel { get; set; }
    public required List<NotifySendResult> SendResults { get; set; }
}
