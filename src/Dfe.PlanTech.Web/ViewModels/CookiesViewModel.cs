using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CookiesViewModel
{
    public List<ContentfulEntry> BeforeTitleContent { get; init; } = [];

    public ComponentTitleEntry Title { get; init; } = null!;

    public List<ContentfulEntry> Content { get; init; } = null!;

    public DfeCookieModel Cookie { get; init; } = new DfeCookieModel();

    public string ReferrerUrl { get; init; } = null!;
}
