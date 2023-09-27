using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Web.Exceptions;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class RecommendationsController : BaseController<RecommendationsController>
{
  public RecommendationsController(ILogger<RecommendationsController> logger) : base(logger)
  {
  }

  [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}", Name = "GetRecommendation")]
  public async Task<IActionResult> GetRecommendation(string sectionSlug,
                                                     string recommendationSlug,
                                                     [FromServices] UserProgressValidator userProgressValidator,
                                                     [FromServices] GetPageQuery getPageQuery,
                                                     [FromServices] IUser user,
                                                     CancellationToken cancellationToken)
  {
    var journeyStatus = await userProgressValidator.GetJourneyStatusForSection(sectionSlug, cancellationToken);

    switch (journeyStatus.Status)
    {
      case JourneyStatus.CheckAnswers: return this.RedirectToCheckAnswers(sectionSlug);
      case JourneyStatus.NotStarted:
      case JourneyStatus.NextQuestion: return new RedirectToActionResult("GetQuestionBySlug", "Questions", new { sectionSlug, questionSlug = journeyStatus.NextQuestion!.Slug });
    }

    if (journeyStatus.Maturity == null)
    {
      throw new Exception("Maturity is null - shouldn't be");
    }

    var recommendationForSlug = journeyStatus.Section.Recommendations.FirstOrDefault(recommendation => recommendation.Page.Slug == recommendationSlug) ??
                throw new ContentfulDataUnavailableException($"Couldn't find recommendation with slug {recommendationSlug} under {sectionSlug}");

    var recommendationForMaturity = journeyStatus.Section.GetRecommendationForMaturity(journeyStatus.Maturity) ??
                                    throw new ContentfulDataUnavailableException("Missing recommendation page");

    if (recommendationForMaturity.Sys.Id != recommendationForSlug.Sys.Id)
    {
      return RedirectToAction(nameof(GetRecommendation), new { sectionSlug, recommendationSlug = recommendationForMaturity.Page.Slug });
    }

    var page = await getPageQuery.GetPageBySlug(recommendationSlug, cancellationToken);

    var viewModel = CreatePageModel(page, user);

    return View("~/Views/Pages/Page.cshtml", viewModel);
  }

  private PageViewModel CreatePageModel(Page page, IUser user)
  {
    ViewData["Title"] = System.Net.WebUtility.HtmlDecode(page.Title?.Text) ?? "Plan Technology For Your School";

    var viewModel = new PageViewModel()
    {
      Page = page,
    };

    viewModel.TryLoadOrganisationName(HttpContext, user, logger);

    return viewModel;
  }
}