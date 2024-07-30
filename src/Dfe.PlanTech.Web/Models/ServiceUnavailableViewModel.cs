namespace Dfe.PlanTech.Web.Models;

public class ServiceUnavailableViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? ContactUsEmail { get; set; }
}
