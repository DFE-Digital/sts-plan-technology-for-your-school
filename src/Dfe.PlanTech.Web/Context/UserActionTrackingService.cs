using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Web.Context.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Web.Context;

public class UserActionTrackingService(
    IUserActionRepository userActionRepository,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUser currentUser) : IUserActionTrackingService
{
    public async Task RecordAsync()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext found.");

        if (httpContext.Items.ContainsKey(UserActionIdConstants.HttpContextItemKey))
        {
            return;
        }

        var userId = currentUser.UserId;
        if (userId is null)
        {
            throw new InvalidOperationException("Current user ID was not found.");
        }

        var userActionId = Guid.NewGuid();

        var userAction = new UserActionEntity
        {
            Id = userActionId,
            UserId = userId.Value,
            EstablishmentId = await currentUser.GetActiveEstablishmentIdAsync(),
            MatEstablishmentId = currentUser.IsMat ? currentUser.UserOrganisationId : null,
            RequestedUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}",
        };

        httpContext.Items[UserActionIdConstants.HttpContextItemKey] = userActionId;

        await userActionRepository.CreateAsync(userAction);
    }
}
