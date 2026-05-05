using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Interfaces;

namespace Dfe.PlanTech.Web.Context;

public class UserActionIdAccessor(IHttpContextAccessor httpContextAccessor) : IUserActionIdAccessor
{
    public Guid GetUserActionId()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext found.");

        if (httpContext.Items.TryGetValue(UserActionIdConstants.HttpContextItemKey, out var value)
            && value is Guid userActionId)
        {
            return userActionId;
        }

        throw new InvalidOperationException("User Action Id was not found in the current request.");
    }
}
