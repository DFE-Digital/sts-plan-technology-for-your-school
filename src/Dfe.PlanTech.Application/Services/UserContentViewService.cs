using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Services;

public class UserContentViewService(
    ILogger<UserActionTrackingService> logger,
    ICurrentUserProvider currentUser,
    IUserContentViewRepository userContentViewRepository
) : IUserContentViewService
{
    private readonly ILogger<UserActionTrackingService> logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICurrentUserProvider _currentUser =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly IUserContentViewRepository _userContentViewRepository =
        userContentViewRepository
        ?? throw new ArgumentNullException(nameof(userContentViewRepository));

    public async Task RecordContentViewAsync(string contentfulRef)
    {
        await _userContentViewRepository.CreateAsync(_currentUser.UserId, contentfulRef);
    }

    public async Task<int> GetNumberOfTimesContentViewedByUserAsync(string contentfulRef)
    {
        return await _userContentViewRepository.GetNumberOfTimesContentViewedByUserAsync(
            _currentUser.UserId,
            contentfulRef
        );
    }
}
