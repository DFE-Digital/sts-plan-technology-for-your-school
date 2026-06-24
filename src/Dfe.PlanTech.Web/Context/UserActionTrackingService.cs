using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Middleware;

namespace Dfe.PlanTech.Web.Context;

public class UserActionTrackingService(
    IUserActionRepository userActionRepository,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUser currentUser,
    ILogger<UserActionTrackingService> logger
) : IUserActionTrackingService
{
    public async Task RecordAsync()
    {
        var httpContext =
            httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext found.");

        if (httpContext.Items.ContainsKey(UserActionIdConstants.HttpContextItemKey))
        {
            return;
        }

        var userId = currentUser.UserId;

        if (userId is null)
        {
            logger.LogInformation("No current user id found");
            return;
        }

        var userActionId = Guid.NewGuid();

        var userAction = new UserActionEntity
        {
            Id = userActionId,
            SessionId = currentUser.SessionId,
            UserId = userId.Value,
            EstablishmentId = await currentUser.GetActiveEstablishmentIdAsync(),
            MatEstablishmentId = currentUser.IsMat ? currentUser.UserOrganisationId : null,
            RequestedUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}",
        };

        httpContext.Items[UserActionIdConstants.HttpContextItemKey] = userActionId;

        await userActionRepository.CreateAsync(userAction);
    }

    public async Task<SqlUserActionDto?> GetAsync(Guid id)
    {
        var userActionEntity = await userActionRepository.GetUserActionAsync(id);

        if (userActionEntity is null)
        {
            return null;
        }

        return new SqlUserActionDto
        {
            Id = userActionEntity.Id,
            SessionId = userActionEntity.SessionId,
            UserId = userActionEntity.UserId,
            EstablishmentId = userActionEntity.EstablishmentId,
            MatEstablishmentId = userActionEntity.MatEstablishmentId,
            RequestedUrl = userActionEntity.RequestedUrl,
        };
    }
}
