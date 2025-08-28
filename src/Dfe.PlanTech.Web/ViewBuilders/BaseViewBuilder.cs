using System.Security.Authentication;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class BaseViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    CurrentUser currentUser)
{
    protected readonly ILogger<BaseViewBuilder> Logger = logger;
    protected IContentfulService ContentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    protected CurrentUser CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    protected int GetUserIdOrThrowException()
    {
        return CurrentUser.UserId ?? throw new AuthenticationException("User is not authenticated");
    }

    protected int GetEstablishmentIdOrThrowException()
    {
        return CurrentUser.EstablishmentId ?? throw new InvalidDataException(nameof(currentUser.EstablishmentId));
    }

    protected string GetDsiReferenceOrThrowException()
    {
        return CurrentUser.DsiReference ?? throw new AuthenticationException("User is not authenticated");
    }

    protected async Task<CategorySectionRecommendationViewModel> BuildCategorySectionRecommendationViewModel(
        QuestionnaireSectionEntry section,
        SqlSectionStatusDto? sectionStatus
    )
    {
        if (string.IsNullOrEmpty(sectionStatus?.LastMaturity))
        {
            return new CategorySectionRecommendationViewModel();
        }

        try
        {
            var recommendationIntro = await ContentfulService.GetSubtopicRecommendationIntroAsync(section.Id, sectionStatus.LastMaturity);
            if (recommendationIntro == null)
            {
                return new CategorySectionRecommendationViewModel
                {
                    NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
                };
            }

            return new CategorySectionRecommendationViewModel
            {
                RecommendationSlug = recommendationIntro.Slug,
                RecommendationDisplayName = recommendationIntro.Header.Text,
                SectionSlug = section.InterstitialPage?.Slug,
                Viewed = sectionStatus.HasBeenViewed
            };
        }
        catch (Exception e)
        {
            Logger.LogError(
                e,
                "An exception has occurred while trying to retrieve the recommendation for Section {sectionName}, with the message {errMessage}",
                section.Name,
                e.Message
            );
            return new CategorySectionRecommendationViewModel
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }
    }

    protected bool IsChangeAnswersFlow(string? returnTo)
    {
        return returnTo?.Equals(nameof(ReviewAnswersController).GetControllerNameSlug()) ?? false;
    }
}
