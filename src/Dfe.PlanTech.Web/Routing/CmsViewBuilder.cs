using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Models.QaVisualiser;
using Dfe.PlanTech.Web.ViewModels.QaVisualiser;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing
{
    public class CmsViewBuilder(
        ContentfulService contentfulService,
        RecommendationService recommendationService
    )
    {
        private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
        private readonly RecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));

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
                    .Where(a => a.Sys.Id is not null)
                    .Select(a => new ChunkModel(a.Sys.Id!, chunk.Header)))
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
}
