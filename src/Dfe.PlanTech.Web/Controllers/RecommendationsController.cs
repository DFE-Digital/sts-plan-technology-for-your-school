using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class RecommendationsController(
    ILogger<RecommendationsController> logger,
    IRecommendationsViewBuilder recommendationsViewBuilder
) : BaseController<RecommendationsController>(logger)
{
    private readonly IRecommendationsViewBuilder _recommendationsViewBuilder =
        recommendationsViewBuilder
        ?? throw new ArgumentNullException(nameof(recommendationsViewBuilder));

    [ValidateMatSelected]
    [HttpGet(
        "{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}",
        Name = "GetSingleRecommendation"
    )]
    public async Task<IActionResult> GetSingleRecommendation(
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        return await _recommendationsViewBuilder.RouteToSingleRecommendation(
            this,
            categorySlug,
            sectionSlug,
            chunkSlug,
            false
        );
    }

    [ValidateMatSelected]
    [HttpGet(
        "{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}/print",
        Name = "PrintSingleRecommendation"
    )]
    public async Task<IActionResult> PrintSingleRecommendation(
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        return await _recommendationsViewBuilder.RouteToPrintSingle(
            this,
            categorySlug,
            sectionSlug,
            chunkSlug
        );
    }

    [ValidateMatSelected]
    [HttpGet(
        "{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}/print-all",
        Name = "PrintAllRecommendations"
    )]
    public async Task<IActionResult> PrintAllRecommendations(
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        return await _recommendationsViewBuilder.RouteToPrintAll(
            this,
            categorySlug,
            sectionSlug,
            chunkSlug
        );
    }

    [ValidateMatSelected]
    [HttpGet(
        "{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}/share",
        Name = "ShareSingleRecommendation"
    )]
    public async Task<IActionResult> ShareSingleRecommendation(
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        return await _recommendationsViewBuilder.RouteToShareRecommendationAsync(
            this,
            categorySlug,
            sectionSlug,
            chunkSlug
        );
    }

    [HttpPost(
        "{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}/share",
        Name = "ShareSingleRecommendation"
    )]
    public async Task<IActionResult> PostShareSingleRecommendation(
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        [FromForm] ShareByEmailInputViewModel inputModel
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        return await _recommendationsViewBuilder.RouteToShareRecommendationAsync(
            this,
            categorySlug,
            sectionSlug,
            chunkSlug,
            inputModel
        );
    }

    [HttpPost("{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}/update-status")]
    public async Task<IActionResult> UpdateRecommendationStatus(
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        [FromForm] string selectedStatus,
        [FromForm] string? notes
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        if (string.IsNullOrWhiteSpace(selectedStatus))
        {
            try
            {
                TempData["StatusUpdateError"] = "Select a status";
            }
            catch
            { /* TempData will be null during unit testing and without this some tests fail. */
            }

            return PageRedirecter.RedirectToGetSingleRecommendation(
                this,
                categorySlug,
                sectionSlug,
                chunkSlug
            );
        }

        return await _recommendationsViewBuilder.UpdateRecommendationStatusAsync(
            this,
            categorySlug,
            sectionSlug,
            chunkSlug,
            selectedStatus,
            notes
        );
    }
}
