using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class GroupsSelectorViewModel
{
    public string GroupName { get; set; } = null!;

    public List<SqlEstablishmentLinkDto> GroupEstablishments { get; set; } = [];

    public List<ContentfulEntry> BeforeTitleContent { get; init; } = [];

    public ComponentTitleEntry Title { get; init; } = null!;

    public List<ContentfulEntry> Content { get; init; } = [];

    public string? TotalRecommendations { get; set; }

    public string? ProgressRetrievalErrorMessage { get; init; }

    public string? ContactLinkHref { get; set; }
}
