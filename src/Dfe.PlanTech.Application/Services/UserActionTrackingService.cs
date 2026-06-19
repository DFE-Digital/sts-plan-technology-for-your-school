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
    IUserActionRepository userActionRepository,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserProvider currentUser
) : IUserActionTrackingService
{
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public Task RecordActionAsync()
    {
        var request =
            httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException("No active HttpContext found.");

        return RecordAsync($"{request.Path}{request.QueryString}");
    }

    public Task RecordBannerViewAsync(string bannerId)
    {
        var contentfulEntryUrl =
            $"https://app.contentful.com/spaces/py5afvqdlxgo/environments/development/entries/{bannerId}";

        return RecordAsync(contentfulEntryUrl);
    }

    public async Task<int> GetNumberOfTimesBannerViewedByUserAsync(string bannerId)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            logger.LogInformation("No current user ID found");
            return 0;
        }

        return await userActionRepository.GetNumberOfTimesBannerViewedByUserAsync(
            userId.Value,
            bannerId
        );
    }

    private async Task RecordAsync(string requestedUrl)
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
            logger.LogInformation("No current user ID found");
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

        await userActionRepository.CreateAsync(userAction);
    }
}
