using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class GroupsDashboardViewComponentViewModel
{
    public required ContentfulEntry Description { get; set; }

    public List<GroupsCategorySectionViewModel> GroupsCategorySections { get; init; } = [];

    public string? NoSectionsErrorRedirectUrl { get; set; }

    public string? ProgressRetrievalErrorMessage { get; init; }
}
