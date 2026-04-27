using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfe.PlanTech.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateMatSelectedAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

        if (currentUser is { IsAuthenticated: true, IsMat: true, GroupSelectedSchoolUrn: null })
        {
                context.Result = new RedirectToRouteResult(
                    new { controller = "Groups", action = "GetSelectASchoolView" }
                );
                return;
        }

        base.OnActionExecuting(context);
    }
}

