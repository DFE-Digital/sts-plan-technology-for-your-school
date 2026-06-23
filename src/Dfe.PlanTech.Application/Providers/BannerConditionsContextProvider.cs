using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Providers;

public class BannerConditionsContextProvider(
    ILogger<BannerConditionsContextProvider> logger,
    ICurrentUserProvider currentUser,
    ISubmissionService submissionService,
    IRecommendationService recommendationService,
    IUserContentViewService userContentViewService
) : IBannerConditionsContextProvider
{
    private readonly ILogger<BannerConditionsContextProvider> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICurrentUserProvider _currentUser =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IRecommendationService _recommendationService =
        recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
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

    private async Task<bool> ShouldShowBanner(ComponentNotificationBannerEntry banner)
    {
        if (banner.DisplayFrom.HasValue && banner.DisplayFrom.Value >= DateTime.UtcNow)
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
            && _currentUser.OrganisationCategoryIdMatchesAny(
                DsiConstants.OrganisationEstablishmentCategoryIds
            )
        )
        {
            return false; // Banner is not for school users, and the user is a school user
        }

        if (
            banner.ShowToGroupUsers == false
            && _currentUser.OrganisationCategoryIdMatchesAny(
                DsiConstants.OrganisationGroupCategoryIds
            )
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

        if (submission == null)
        {
            return condition.ShowIfStatusUnknown == true || condition.ShowIfNotStarted == true;
        }

        return submission.Status switch
        {
            SubmissionStatus.None when condition.ShowIfNotStarted == false => false,
            SubmissionStatus.NotStarted when condition.ShowIfNotStarted == false => false,
            SubmissionStatus.InProgress when condition.ShowIfInProgress == false => false,
            SubmissionStatus.CompleteNotReviewed when condition.ShowIfCompleted == false => false,
            SubmissionStatus.CompleteReviewed when condition.ShowIfCompleted == false => false,
            _ => true,
        };
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

        if (condition.ShowIfStatusUnknown == false && recommendation is null)
        {
            return false;
        }

        // We need a status for the rest of the conditions, so return early if recommendation is null
        if (recommendation == null)
        {
            return false;
        }

        if (
            condition.ShowIfNotStarted == false
            && recommendation.NewStatus == RecommendationStatus.NotStarted
        )
        {
            return false;
        }

        if (
            condition.ShowIfInProgress == false
            && recommendation.NewStatus == RecommendationStatus.InProgress
        )
        {
            return false;
        }

        if (
            condition.ShowIfCompleted == false
            && recommendation.NewStatus == RecommendationStatus.Complete
        )
        {
            return false;
        }

        return true;
    }
}
