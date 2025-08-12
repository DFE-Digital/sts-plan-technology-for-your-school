using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
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
    public const string ControllerName = "Recommendations";
    public const string GetRecommendationAction = "GetRecommendation";
    public const string GetSingleRecommendationAction = "GetSingleRecommendation";

    [HttpGet("{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}", Name = GetSingleRecommendationAction)]
    public async Task<IActionResult> GetSingleRecommendation(string categorySlug,
                                                         string sectionSlug,
                                                         string chunkSlug,
                                                         [FromServices] IGetRecommendationRouter getRecommendationRouter,
                                                         [FromServices] IGetCategoryQuery getCategoryQuery,
                                                         CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(categorySlug))
            throw new ArgumentNullException(nameof(categorySlug));
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(chunkSlug))
            throw new ArgumentNullException(nameof(chunkSlug));

        var category = await getCategoryQuery.GetCategoryBySlug(categorySlug) ?? throw new ContentfulDataUnavailableException($"Unable to retrieve category with slug {categorySlug}");

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

    [HttpGet("{categorySlug}/{sectionSlug}/recommendations/print", Name = "GetRecommendationChecklist")]
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
}

