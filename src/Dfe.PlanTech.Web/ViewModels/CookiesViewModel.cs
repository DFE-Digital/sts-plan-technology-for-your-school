using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

public class CookiesViewModel
{
    public CmsComponentTitleDto Title { get; init; } = null!;

    public List<CmsEntryDto> Content { get; init; } = null!;

    public DfeCookieModel Cookie { get; init; } = new DfeCookieModel();

    public string ReferrerUrl { get; init; } = null!;
}
