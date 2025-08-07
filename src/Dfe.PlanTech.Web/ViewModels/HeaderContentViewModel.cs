using Dfe.PlanTech.Core.Contentful.Interfaces;

namespace Dfe.PlanTech.Web.ViewModels;

public class HeaderContentViewModel
{
    public int CurrentRecommendationPageCount { get; set; }
    public IHeaderWithContent? Header { get; set; }
    public int? RecommendationCount { get; set; }
    public string? SubmissionDate { get; set; }
    public string? SectionName { get; set; }
}
