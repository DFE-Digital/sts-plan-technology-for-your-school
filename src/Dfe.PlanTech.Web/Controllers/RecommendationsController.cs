using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
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
    public const string ControllerName = "Recommendations";
    public const string GetRecommendationAction = "GetRecommendation";
    public const string GetSingleRecommendationAction = "GetSingleRecommendation";

    [HttpGet("{categorySlug}/{sectionSlug}/recommendation/{recommendationSlug}", Name = GetRecommendationAction)]
    public async Task<IActionResult> GetRecommendation(string categorySlug,
                                                       string sectionSlug,
                                                       string recommendationSlug,
                                                       [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                       CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug))
            throw new ArgumentNullException(nameof(recommendationSlug));

        return await getRecommendationValidator.ValidateRoute(categorySlug,
          sectionSlug,
          false,
          this,
          cancellationToken);
    }

    [HttpGet("{categorySlug}/{sectionSlug}/{chunkSlug}", Name = GetSingleRecommendationAction)]
    public async Task<IActionResult> GetSingleRecommendation(string categorySlug,
                                                         string sectionSlug,
                                                         string chunkSlug,
                                                         [FromServices] IGetRecommendationRouter getRecommendationRouter,
                                                         [FromServices] IGetPageQuery getPageQuery,
                                                         CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(categorySlug))
            throw new ArgumentNullException(nameof(categorySlug));
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(chunkSlug))
            throw new ArgumentNullException(nameof(chunkSlug));

        var categoryLandingPage = await getPageQuery.GetPageBySlug(categorySlug);
        var category = categoryLandingPage?.Content[0] as Category ?? throw new ContentfulDataUnavailableException($"No category landing page found for slug: {categorySlug}");

        var (section, currentChunk, allChunks) = await getRecommendationRouter.GetSingleRecommendation(sectionSlug, chunkSlug, this, cancellationToken);
        var currentChunkIndex = allChunks.IndexOf(currentChunk);
        var previousChunk = currentChunkIndex > 0
                            ? allChunks[currentChunkIndex - 1]
                            : null;
        var nextChunk = currentChunkIndex != allChunks.Count - 1
                            ? allChunks[currentChunkIndex + 1]
                            : null;

        var viewModel = new SingleRecommendationViewModel
        {
            CategoryName = category.Header.Text,
            CategorySlug = categorySlug,
            Section = section,
            Chunks = allChunks,
            CurrentChunk = currentChunk,
            PreviousChunk = previousChunk,
            NextChunk = nextChunk,
            CurrentChunkPosition = currentChunkIndex + 1,
            TotalChunks = allChunks.Count
        };
        return View("~/Views/Recommendations/SingleRecommendation.cshtml", viewModel);
    }

    [HttpGet("{sectionSlug}/recommendation/preview/{maturity?}", Name = "GetRecommendationPreview")]
    public async Task<IActionResult> GetRecommendationPreview(string sectionSlug,
                                                              string? maturity,
                                                              [FromServices] ContentfulOptions contentfulOptions,
                                                              [FromServices] IGetRecommendationRouter getRecommendationRouter,
                                                              CancellationToken cancellationToken)
    {
        if (!contentfulOptions.UsePreviewApi)
        {
            return new RedirectResult(UrlConstants.HomePage);
        }

        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        return await getRecommendationRouter.GetRecommendationPreview(sectionSlug, maturity, this, cancellationToken);
    }

    [HttpGet("{categorySlug}/{sectionSlug}/print", Name = "GetRecommendationChecklist")]
    public async Task<IActionResult> GetRecommendationChecklist(string categorySlug,
                                                                string sectionSlug,
                                                                [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                                CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        return await getRecommendationValidator.ValidateRoute(categorySlug, sectionSlug,
            true,
            this,
            cancellationToken);
    }

    [HttpGet("from-section/{sectionSlug}")]
    public async Task<IActionResult> FromSection(
        string sectionSlug,
        [FromServices] IGetRecommendationRouter getRecommendationRouter,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        var recommendationSlug = await getRecommendationRouter.GetRecommendationSlugForSection(sectionSlug, cancellationToken);
        return RedirectToAction(nameof(GetRecommendation), new { recommendationSlug });
    }
}

