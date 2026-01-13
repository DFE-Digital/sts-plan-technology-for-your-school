using System.Text;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class GridContainerRendererTests : CardTests
{
    private readonly ICardContentPartRenderer _cardContentPartRendererSubstitute = Substitute.For<ICardContentPartRenderer>();

    [Fact]
    public void CheckNullContentReturnsEmptyString()
    {
        var renderer = new GridContainerRenderer(_cardContentPartRendererSubstitute);
        var result = renderer.ToHtml(null);

        Assert.Equal(string.Empty, result?.ToString());
    }

    [Fact]
    public void CheckEmptyContentReturnsEmptyString()
    {
        var renderer = new GridContainerRenderer(_cardContentPartRendererSubstitute);
        var result = renderer.ToHtml([]);

        Assert.Equal(string.Empty, result?.ToString());
    }

    [Fact]
    public void CheckCardsRenderedCorrectly()
    {
        var renderer = new GridContainerRenderer(_cardContentPartRendererSubstitute);

        var stringBuilder = new StringBuilder();

        stringBuilder.Append("<div class=\"govuk-grid-row\">");
        stringBuilder.Append("<div class=\"govuk-grid-column-two-thirds-from-desktop\">");
        stringBuilder.Append("<div class=\"dfe-grid-container\">");
        stringBuilder.Append("</div></div></div>");

        _cardContentPartRendererSubstitute.AddHtml(GetFourthCard(), new StringBuilder());

        var result = renderer.ToHtml(GetCards());

        Assert.Equal(GetCardContainerWithoutCards().ToString(), result);
    }

    private StringBuilder GetCardContainerWithoutCards()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("<div class=\"govuk-grid-row\">");
        stringBuilder.Append("<div class=\"govuk-grid-column-two-thirds-from-desktop\">");
        stringBuilder.Append("<div class=\"dfe-grid-container\">");
        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }
}
