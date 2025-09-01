using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.QaVisualiser;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CmsViewBuilder(
    IContentfulService contentfulService,
    IRecommendationService recommendationService
) : ICmsViewBuilder
{
    private readonly IContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private readonly IRecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));

    public async Task<IEnumerable<SectionViewModel>> GetAllSectionsAsync()
    {
        var sections = await _contentfulService.GetAllSectionsAsync();
        return sections.Select(s => new SectionViewModel(s));
    }

    public async Task<IActionResult> GetChunks(Controller controller, int? page)
    {
        var pageNumber = page ?? 1;
        var total = await _recommendationService.GetRecommendationChunkCount(pageNumber);
        var entries = await _recommendationService.GetPaginatedRecommendationEntries(pageNumber);

        var chunkModels = entries
            .SelectMany(chunk => chunk.Answers
                .Where(a => a.Id is not null)
                .Select(a => new ChunkModel(a.Id!, chunk.HeaderText)))
            .ToList();

        var resultModel = new PagedResultViewModel<ChunkModel>
        {
            Page = pageNumber,
            Total = total,
            Items = chunkModels
        };

        return controller.Ok(resultModel);
    }
}
