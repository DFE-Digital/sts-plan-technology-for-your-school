using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
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

        var request =
            httpContext?.Request
            ?? throw new InvalidOperationException("No active HttpRequest found.");

        var userId = _currentUser.UserId;
        if (userId is null)
        {
            logger.LogInformation("No current user ID found");
            return;
        }

        var requestedUrl = $"{request.Path}{request.QueryString}";

        if (
            httpContext.Items.TryGetValue(UserActionIdConstants.HttpContextItemKey, out var value)
            && value is Guid
        )
        {
            logger.LogInformation(
                "User action already recorded for request {RequestPath}.",
                requestedUrl
            );
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
            RequestedUrl = requestedUrl,
        };

        httpContext.Items[UserActionIdConstants.HttpContextItemKey] = userActionId;

        await _userActionRepository.CreateAsync(userAction);
    }
}
