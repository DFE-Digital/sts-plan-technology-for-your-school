using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models;

public class PageViewModel
{
    public required Page Page { get; init; }

    public void TryLoadOrganisationName(HttpContext httpContext, IUser user, ILogger logger)
    {
        if (!Page.DisplayOrganisationName) return;

        if (httpContext.User.Identity?.IsAuthenticated == false)
        {
            logger.LogWarning("Tried to display establishment on {page} but user is not authenticated", Page.Title?.Text ?? Page.Sys.Id);
            return;
        }

        var establishment = user.GetOrganisationData();

        Page.OrganisationName = establishment.OrgName;
    }
}
