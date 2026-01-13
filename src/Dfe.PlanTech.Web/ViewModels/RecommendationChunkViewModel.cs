using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Web.ViewModels;

public class RecommendationChunkViewModel
{
    public List<ContentfulEntry> Content { get; set; } = [];
    public string Header { get; init; } = null!;
    public string HeaderText => Header;
    public DateTime LastUpdated { get; set; }
    public string LastUpdatedFormatted => LastUpdated.ToString("d MMMM yyyy") ?? RecommendationConstants.DefaultLastUpdatedText;
    public RecommendationStatus Status { get; set; }
    public string StatusText => Status.GetDisplayName();
    public string StatusTagClass => Status.GetCssClass();
    public string LinkText => HeaderText;
    public string Slug { get; set; } = null!;
}
