using Dfe.PlanTech.Core.Contentful.Interfaces;

namespace Dfe.PlanTech.Web.ViewModels;

public class HeaderContentViewModel
{
    public IHeaderWithContent? Header { get; set; }
    public string? SubmissionDate { get; set; }
    public string? SectionName { get; set; }
}
