using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.QaVisualiser;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CmsViewBuilder(IContentfulService contentfulService) : ICmsViewBuilder
{
    private readonly IContentfulService _contentfulService =
        contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));

    public async Task<IEnumerable<SectionViewModel>> GetAllSectionsAsync()
    {
        var sections = await _contentfulService.GetAllSectionsAsync();
        return sections.Select(s => new SectionViewModel(s));
    }

    public async Task<IActionResult> GetChunks(Controller controller, int? page)
    {
        var sections = await _contentfulService.GetAllSectionsAsync();

        var sectionAllAnswers = sections
            .SelectMany(s => s.Questions)
            .SelectMany(q => q.Answers)
            .Select(a => a.Id)
            .ToList();

        var pageNumber = page ?? 1;
        var total = await _contentfulService.GetRecommendationChunkCountAsync(pageNumber);
        var entries = await _contentfulService.GetPaginatedRecommendationEntriesAsync(pageNumber);
        List<ChunkModel> chunkModels = new();

        chunkModels = entries
            .Select(a =>
            {
                // Validate and retrieve the answer that is within the sectionAllAnswers so we don't retrieve "RETIRED" answers
                var completingAnswer = a.CompletingAnswers.FirstOrDefault(ans =>
                    sectionAllAnswers.Contains(ans.Id)
                );

                var inProgressAnswer = a.InProgressAnswers.FirstOrDefault(ans =>
                    sectionAllAnswers.Contains(ans.Id)
                );

                return new ChunkModel(
                    completingAnswer?.Id ?? "",
                    inProgressAnswer?.Id ?? "",
                    a.HeaderText
                );
            })
            .ToList();

        var resultModel = new PagedResultViewModel<ChunkModel>
        {
            Page = pageNumber,
            Total = total,
            Items = chunkModels,
        };

        return controller.Ok(resultModel);
    }
}
