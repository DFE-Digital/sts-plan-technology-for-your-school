using System.Security.Authentication;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class BaseViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUser currentUser
)
{
    protected readonly ILogger<BaseViewBuilder> Logger = logger;
    protected IContentfulService ContentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    protected ICurrentUser CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

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

    protected CategorySectionRecommendationViewModel BuildCategorySectionRecommendationViewModel(
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
            return new CategorySectionRecommendationViewModel
            {
                SectionSlug = section.InterstitialPage?.Slug,
                SectionName = section.Name,
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
}
