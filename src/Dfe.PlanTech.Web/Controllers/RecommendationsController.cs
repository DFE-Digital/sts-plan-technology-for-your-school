using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class RecommendationsController(ILogger<RecommendationsController> logger)
    : BaseController<RecommendationsController>(logger)
{
    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}", Name = "GetRecommendation")]
    public async Task<IActionResult> GetRecommendation(string sectionSlug,
                                                       string recommendationSlug,
                                                       [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                       CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug))
            throw new ArgumentNullException(nameof(recommendationSlug));

        return await getRecommendationValidator.ValidateRoute(sectionSlug,
          recommendationSlug,
          false,
          this,
          cancellationToken);
    }

    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}/preview/{maturity?}", Name = "GetRecommendationPreview")]
    public async Task<IActionResult> GetRecommendationPreview(string sectionSlug,
                                                       string recommendationSlug,
                                                       string? maturity,
                                                       [FromServices] ContentfulOptions contentfulOptions,
                                                       [FromServices] IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery,
                                                       [FromServices] IGetSectionQuery getSectionQuery,
                                                       CancellationToken cancellationToken)
    {
        if (!contentfulOptions.UsePreview)
        {
            return new RedirectToActionResult("GetRecommendation", "Recommendations", new { sectionSlug, recommendationSlug });
        }

        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug))
            throw new ArgumentNullException(nameof(recommendationSlug));

        var subtopic = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken);

        if (subtopic == null)
        {
            return new NotFoundResult();
        }

        var recommendation = await getSubTopicRecommendationQuery.GetSubTopicRecommendation(subtopic.Sys.Id, cancellationToken);

        if (recommendation == null)
        {
            logger.LogError("Couldn't find recommendation for section slug {SectionSlug}", sectionSlug);
            return new NotFoundResult();
        }

        var intro = recommendation.Intros.FirstOrDefault(intro => intro.Maturity == maturity) ?? recommendation.Intros.First();

        var viewModel = new RecommendationsViewModel()
        {
            SectionName = subtopic.Name,
            Intro = intro,
            Chunks = recommendation.Section.Chunks,
            Slug = recommendationSlug,
        };

        return View("~/Views/Recommendations/Recommendations.cshtml", viewModel);
    }

    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}/print", Name = "GetRecommendationChecklist")]
    public async Task<IActionResult> GetRecommendationChecklist(string sectionSlug,
                                                                string recommendationSlug,
                                                                [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                                CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug))
            throw new ArgumentNullException(nameof(recommendationSlug));

        return await getRecommendationValidator.ValidateRoute(sectionSlug,
            recommendationSlug,
            true,
            this,
            cancellationToken);
    }
}

