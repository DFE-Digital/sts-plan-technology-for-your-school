using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class RecommendationChunksViewComponentTest
{
    [Fact]
    public void Recommendation_chunks_view_component_not_null()
    {
        IEnumerable<RecommendationChunk> recommendationChunks = new List<RecommendationChunk>
        {
            new RecommendationChunk()
            {
                Title = "Title Text 1",
                Header = new() { Text = "Header Chunk Text 1", Size = HeaderSize.Medium },
                Content = [],
                Answers = []
            },
            new RecommendationChunk()
            {
                Title = "Title Text 2",
                Header = new() { Text = "Header Chunk Text 2", Size = HeaderSize.Medium },
                Content = [],
                Answers = []
            }
        };

        var recommendationChunksViewComponent = new RecommendationChunksViewComponent();

        var result = recommendationChunksViewComponent.Invoke(recommendationChunks);

        var model = (result as ViewViewComponentResult)?.ViewData?.Model;

        Assert.NotNull(model);
    }
}