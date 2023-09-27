using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Exceptions;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class GetRecommendationValidator
{
  private readonly IGetPageQuery _getPageQuery;
  private readonly ILogger<GetRecommendationValidator> _logger;
  private readonly IUser _user;

  public GetRecommendationValidator(IGetPageQuery getPageQuery, ILogger<GetRecommendationValidator> logger, IUser user)
  {
    _getPageQuery = getPageQuery;
    _logger = logger;
    _user = user;
  }

  public async Task<IActionResult> ValidateRoute(string sectionSlug,
                                                 string recommendationSlug,
                                                 UserJourneyRouter router,
                                                 RecommendationsController controller,
                                                 CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentException($"'{nameof(sectionSlug)}' cannot be null or empty.");
    if (string.IsNullOrEmpty(recommendationSlug)) throw new ArgumentException($"'{nameof(recommendationSlug)}' cannot be null or empty.");

    switch (router.Status)
    {
      case JourneyStatus.CheckAnswers: return controller.RedirectToCheckAnswers(sectionSlug);
      case JourneyStatus.NotStarted:
      case JourneyStatus.NextQuestion: return new RedirectToActionResult("GetQuestionBySlug", "Questions", new { sectionSlug, questionSlug = router.NextQuestion!.Slug });
      default:
        {

          if (router.SectionStatus?.Maturity == null) throw new InvalidDataException("Maturity is null - shouldn't be");

          var recommendationForSlug = router.Section!.Recommendations.FirstOrDefault(recommendation => recommendation.Page.Slug == recommendationSlug) ??
                                        throw new ContentfulDataUnavailableException($"Couldn't find recommendation with slug {recommendationSlug} under {sectionSlug}");

          var recommendationForMaturity = router.Section.GetRecommendationForMaturity(router.SectionStatus.Maturity) ??
                                          throw new ContentfulDataUnavailableException("Missing recommendation page");

          if (recommendationForMaturity.Sys.Id != recommendationForSlug.Sys.Id)
          {
            return controller.RedirectToAction(RecommendationsController.GetRecommendationAction,
                                               new { sectionSlug, recommendationSlug = recommendationForMaturity.Page.Slug });
          }

          var page = await _getPageQuery.GetPageBySlug(recommendationSlug, cancellationToken);

          var viewModel = new PageViewModel(page, controller, _user, _logger);

          return controller.View("~/Views/Pages/Page.cshtml", viewModel);
        }
    }
  }
}