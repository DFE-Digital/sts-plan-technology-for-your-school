using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class RecommendationIntroViewComponentTests
{
    [Fact]
    public void Recommendation_Intro_view_component_not_null()
    {
        RecommendationIntro recommendationIntro = new RecommendationIntro()
        {
            Header = new() { Text = "Header Intro Text", Size = HeaderSize.Medium },
            Maturity = "High",
            Content = []
        };
        var recommendationChunksViewComponent = new RecommendationIntroViewComponent();

        var result = recommendationChunksViewComponent.Invoke(recommendationIntro);

        var model = (result as ViewViewComponentResult)?.ViewData?.Model;

        Assert.NotNull(model);
    }
}