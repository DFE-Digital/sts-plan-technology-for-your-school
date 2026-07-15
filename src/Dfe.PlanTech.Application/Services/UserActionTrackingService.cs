using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Services;

public class UserActionTrackingService(
    ILogger<UserActionTrackingService> logger,
    ICurrentUserProvider currentUser,
    IHttpContextAccessor httpContextAccessor,
    IUserActionRepository userActionRepository
) : IUserActionTrackingService
{
    private readonly ILogger<UserActionTrackingService> logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICurrentUserProvider _currentUser =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly IUserActionRepository _userActionRepository =
        userActionRepository ?? throw new ArgumentNullException(nameof(userActionRepository));

    public async Task RecordActionAsync()
    {
        var httpContext =
            _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext found.");

        var request = httpContext.Request;

        var userId = _currentUser.UserId;

        if (userId is null)
        {
            logger.LogInformation("No current user ID found");
            return;
        }

        var requestedUrl = $"{request.Path}{request.QueryString}";

        if (
            httpContext.Items.TryGetValue(
                UserActionIdConstants.RecordedHttpContextItemKey,
                out var recorded
            )
            && recorded is true
        )
        {
            logger.LogInformation(
                "User action already recorded for request {RequestPath}.",
                requestedUrl
            );

            return;
        }

        var userActionId =
            httpContext.Items.TryGetValue(
                UserActionIdConstants.HttpContextItemKey,
                out var value
            )
            && value is Guid existingUserActionId
                ? existingUserActionId
                : Guid.NewGuid();

        var userAction = new UserActionEntity
        {
            Id = userActionId,
            SessionId = _currentUser.SessionId,
            UserId = userId.Value,
            EstablishmentId = await _currentUser.GetActiveEstablishmentIdAsync(),
            MatEstablishmentId = _currentUser.IsMat
                ? _currentUser.UserOrganisationId
                : null,
            RequestedUrl = requestedUrl,
        };

        await _userActionRepository.CreateAsync(userAction);

        httpContext.Items[
            UserActionIdConstants.RecordedHttpContextItemKey
        ] = true;
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
