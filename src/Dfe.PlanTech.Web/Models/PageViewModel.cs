using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Models;

public class PageViewModel
{
    public Page Page { get; init; }

    public PageViewModel(Page page, Controller controller, IUser user, ILogger logger, bool displayBlueBanner = true)
    {

        controller.ViewData["Title"] = StringExtensions.UseNonBreakingHyphenAndHtmlDecode(page.Title?.Text) ??
                                       "Plan Technology For Your School";
        Page = page;
        DisplayBlueBanner = displayBlueBanner;
        TryLoadOrganisationName(controller.HttpContext, user, logger);
    }

    public void TryLoadOrganisationName(HttpContext httpContext, IUser user, ILogger logger)
    {
        if (!Page.DisplayOrganisationName)
            return;

        if (httpContext.User.Identity?.IsAuthenticated == false)
        {
            logger.LogWarning("Tried to display establishment on {page} but user is not authenticated", Page.Title?.Text ?? Page.Sys.Id);
            return;
        }

        var establishment = user.GetOrganisationData();

        Page.OrganisationName = establishment.OrgName;
    }

    public bool DisplayBlueBanner { get; set; }
}
