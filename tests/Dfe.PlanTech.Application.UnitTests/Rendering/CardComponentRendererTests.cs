using System.Text;
using Dfe.PlanTech.Application.Rendering;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class CardComponentRendererTests : CardTests
{
    [Fact]
    public void ShouldReturnNoChangeWhenNullOrEmpty()
    {
        var renderer = new CardComponentRenderer();
        var stringBuilder = new StringBuilder();

        var result = renderer.AddHtml(null, stringBuilder);

        Assert.Equal(string.Empty, stringBuilder.ToString());
    }

    [Fact]
    public void ShouldReturnRenderedCard1()
    {
        var renderer = new CardComponentRenderer();
        var result = renderer.AddHtml(GetFirstCard(), new StringBuilder());

        Assert.Equal(GetFirstCardView().ToString(), result.ToString());
    }

    [Fact]
    public void ShouldReturnRenderedCard2()
    {
        var renderer = new CardComponentRenderer();
        var result = renderer.AddHtml(GetSecondCard(), new StringBuilder());

        Assert.Equal(GetSecondCardView().ToString(), result.ToString());
    }

    [Fact]
    public void ShouldReturnRenderedCard3()
    {
        var renderer = new CardComponentRenderer();
        var result = renderer.AddHtml(GetThirdCard(), new StringBuilder());

        Assert.Equal(GetThirdCardView().ToString(), result.ToString());
    }

    [Fact]
    public void ShouldReturnRenderedCard4()
    {
        var renderer = new CardComponentRenderer();
        var result = renderer.AddHtml(GetFourthCard(), new StringBuilder());

        Assert.Equal(GetFourthCardView().ToString(), result.ToString());
    }
}
