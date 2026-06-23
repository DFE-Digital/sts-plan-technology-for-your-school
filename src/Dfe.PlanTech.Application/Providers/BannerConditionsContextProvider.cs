using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Providers;

public class BannerConditionsContextProvider(
    ILogger<BannerConditionsContextProvider> logger,
    ICurrentUserProvider currentUser,
    IHttpContextAccessor contextAccessor,
    ISubmissionService submissionService,
    IRecommendationService recommendationService,
    IUserActionTrackingService userActionTrackingService,
    IUserContentViewService userContentViewService
) : IBannerConditionsContextProvider
{
    private readonly ILogger<BannerConditionsContextProvider> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICurrentUserProvider _currentUser =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IRecommendationService _recommendationService =
        recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly IUserActionTrackingService _userActionTrackingService =
        userActionTrackingService
        ?? throw new ArgumentNullException(nameof(userActionTrackingService));
    private readonly IUserContentViewService _userContentViewService =
        userContentViewService ?? throw new ArgumentNullException(nameof(userContentViewService));

    public async Task<bool> RecordViewActionAndGetBannerVisibility(
        ComponentNotificationBannerEntry banner
    )
    {
        var shouldShowBanner = await ShouldShowBanner(banner);
        if (shouldShowBanner)
        {
            await _userContentViewService.RecordContentViewAsync(banner.Id);
        }

        return shouldShowBanner;
    }

    private bool HasCategoryId(IEnumerable<string> categoryIds)
    {
        var organisation = _contextAccessor.HttpContext?.User.Claims.GetOrganisation();
        return organisation?.Category?.Id != null && categoryIds.Contains(organisation.Category.Id);
    }

    private async Task<bool> ShouldShowBanner(ComponentNotificationBannerEntry banner)
    {
        if (banner.DisplayFrom.HasValue && banner.DisplayFrom.Value > DateTime.UtcNow)
        {
            return false; // Banner is not yet active
        }

        if (banner.DisplayTo.HasValue && banner.DisplayTo.Value < DateTime.UtcNow)
        {
            return false; // Banner has expired
        }

        if (banner.NumberOfTimesToShow != null)
        {
            var numberOfTimesShown =
                await _userContentViewService.GetNumberOfTimesContentViewedByUserAsync(banner.Id);
            if (numberOfTimesShown >= banner.NumberOfTimesToShow)
            {
                return false; // Banner has been shown enough times
            }
        }

        if (
            banner.ShowToSchoolUsers == false
            && HasCategoryId([DsiConstants.EstablishmentCategoryId])
        )
        {
            return false; // Banner is not for school users, and the user is a school user
        }

        if (
            banner.ShowToGroupUsers == false
            && HasCategoryId(DsiConstants.OrganisationGroupCategories)
        )
        {
            return false; // Banner is not for group users, and the user is a group user
        }

        if (banner.Conditions == null || !banner.Conditions.Any())
        {
            return true; // No conditions, show the banner
        }

        var establishmentId =
            await _currentUser.GetActiveEstablishmentIdAsync()
            ?? throw new InvalidDataException(nameof(_currentUser.GetActiveEstablishmentIdAsync));

        foreach (var condition in banner.Conditions)
        {
            if (condition.Entry is QuestionnaireSectionEntry sectionEntry)
            {
                var shouldShow = await GetVisibilityForSection(
                    condition,
                    establishmentId,
                    sectionEntry.Id
                );
                switch (shouldShow)
                {
                    case true:
                        continue;
                    case false:
                        return false;
                }
            }

            if (condition.Entry is RecommendationChunkEntry recommendationEntry)
            {
                var shouldShow = await GetVisibilityForRecommendation(
                    condition,
                    establishmentId,
                    recommendationEntry.Id
                );
                switch (shouldShow)
                {
                    case true:
                        continue;
                    case false:
                        return false;
                }
            }
        }

        return true;
    }

    private async Task<bool> GetVisibilityForSection(
        ConditionEntry condition,
        int establishmentId,
        string sectionId
    )
    {
        var submission = await _submissionService.GetLatestCompletedSubmissionBySectionIdAsync(
            establishmentId,
            sectionId
        );

        if (
            (condition.ShowIfStatusUnknown == true || condition.ShowIfNotStarted == true)
            && submission is null
        )
        {
            return true;
        }

        // We need a status for the rest of the conditions, so return early if submission is null
        if (submission == null)
        {
            return false;
        }

        if (condition.ShowIfNotStarted == true && submission.Status != SubmissionStatus.NotStarted)
        {
            return false;
        }

        if (condition.ShowIfInProgress == true && submission.Status != SubmissionStatus.InProgress)
        {
            return false;
        }

        if (
            condition.ShowIfCompleted == true
            && submission.Status != SubmissionStatus.CompleteNotReviewed
            && submission.Status != SubmissionStatus.CompleteReviewed
        )
        {
            return false;
        }

        return true;
    }

    private async Task<bool> GetVisibilityForRecommendation(
        ConditionEntry condition,
        int establishmentId,
        string recommendationId
    )
    {
        var recommendation = await _recommendationService.GetLatestRecommendationHistoryAsync(
            recommendationId,
            establishmentId
        );

        if (condition.ShowIfStatusUnknown == true && recommendation is null)
        {
            return true;
        }

        // We need a status for the rest of the conditions, so return early if recommendation is null
        if (recommendation == null)
        {
            return false;
        }

        if (
            condition.ShowIfNotStarted == true
            && recommendation.NewStatus != RecommendationStatus.NotStarted
        )
        {
            return false;
        }

        if (
            condition.ShowIfInProgress == true
            && recommendation.NewStatus != RecommendationStatus.InProgress
        )
        {
            return false;
        }

        if (
            condition.ShowIfCompleted == true
            && recommendation.NewStatus != RecommendationStatus.Complete
        )
        {
            return false;
        }

        return true;
    }
}
