namespace Dfe.PlanTech.Web.ViewModels;

public class ServiceUnavailableViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? ContactHref { get; set; }
}
