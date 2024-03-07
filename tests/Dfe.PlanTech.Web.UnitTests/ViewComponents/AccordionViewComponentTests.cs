using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class AccordionViewComponentTest
{
    [Fact]
    public void Accordion_view_component_not_null()
    {

        IEnumerable<RecommendationChunk> recommendationChunks = new[]
        {
            new RecommendationChunk()
            {
                Title = "Test Header 1",
                Header = new Header() { Text = "Header 1" },
                Content = new List<ContentComponent>(),
                Answers = new List<Answer>()
            },
            new RecommendationChunk()
            {
                Title = "Test Header 2",
                Header = new Header() { Text = "Header 2" },
                Content = new List<ContentComponent>(),
                Answers = new List<Answer>()
            },
            new RecommendationChunk()
            {
                Title = "Test Header 3",
                Header = new Header() { Text = "Header 3" },
                Content = new List<ContentComponent>(),
                Answers = new List<Answer>()
            },
            new RecommendationChunk()
            {
                Title = "Test Header 4",
                Header = new Header() { Text = "Header 4" },
                Content = new List<ContentComponent>(),
                Answers = new List<Answer>()
            }
        };

        var accordionComponent = new AccordionViewComponent();

        var result = accordionComponent.Invoke(recommendationChunks);

        var model = (result as ViewViewComponentResult)?.ViewData?.Model;
        
        Assert.NotNull(model);
        Assert.Equal(4, (model as List<Accordion>)?.Count);
        Assert.Equal("Test Header 1", (model as List<Accordion>)?[0].Title);
        Assert.Equal("Header 1", (model as List<Accordion>)?[0].Header);

        
    }
}