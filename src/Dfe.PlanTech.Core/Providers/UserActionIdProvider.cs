using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Core.Providers;

public class UserActionIdProvider(IHttpContextAccessor httpContextAccessor) : IUserActionIdProvider
{
    public Guid GetUserActionId()
    {
        var httpContext =
            httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext found.");

        if (
            httpContext.Items.TryGetValue(UserActionIdConstants.HttpContextItemKey, out var value)
            && value is Guid userActionId
        )
        {
            return userActionId;
        }

        throw new InvalidOperationException("User Action Id was not found in the current request.");
    }
}
