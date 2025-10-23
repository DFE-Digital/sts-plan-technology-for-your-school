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

    protected string GetDsiReferenceOrThrowException()
    {
        return CurrentUser.DsiReference ?? throw new AuthenticationException("User is not authenticated");
    }

    protected int GetUserIdOrThrowException()
    {
        return CurrentUser.UserId ?? throw new AuthenticationException("User is not authenticated");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>The PlanTech database ID for the user's organisation (e.g. establishment, or establishment group)</returns>
    /// <exception cref="InvalidDataException"></exception>
    protected int GetUserOrganisationIdOrThrowException()
    {
        return CurrentUser.UserOrganisationId ?? throw new InvalidDataException(nameof(CurrentUser.UserOrganisationId));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>The PlanTech database ID for the selected establishment (e.g. an establishment that a MAT user has selected)</returns>
    /// <exception cref="InvalidDataException"></exception>
    protected int GetActiveEstablishmentIdOrThrowException()
    {
        return CurrentUser.ActiveEstablishmentId ?? throw new InvalidDataException(nameof(CurrentUser.ActiveEstablishmentId));
    }

    protected CategorySectionRecommendationViewModel BuildCategorySectionRecommendationViewModel(
        QuestionnaireSectionEntry section,
        SqlSectionStatusDto? sectionStatus
    )
    {
        try
        {
            return new CategorySectionRecommendationViewModel
            {
                SectionSlug = section.InterstitialPage?.Slug,
                SectionName = section.Name,
                Viewed = sectionStatus?.HasBeenViewed
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
