using System.Text;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers
{
    public class CardComponentRendererTests : CardTests
    {
        [Fact]
        public void ShouldReturnNullWhenNullOrEmpty()
        {
            var renderer = new CardComponent();
            var result = renderer.AddHtml(null, new StringBuilder());

            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnRenderedCard1()
        {
            var renderer = new CardComponent();
            var result = renderer.AddHtml(GetFirstCard(), new StringBuilder());

            Assert.Equal(GetFirstCardView(), result);
        }

        [Fact]
        public void ShouldReturnRenderedCard2()
        {
            var renderer = new CardComponent();
            var result = renderer.AddHtml(GetSecondCard(), new StringBuilder());

            Assert.Equal(GetSecondCardView(), result);
        }

        [Fact]
        public void ShouldReturnRenderedCard3()
        {
            var renderer = new CardComponent();
            var result = renderer.AddHtml(GetThirdCard(), new StringBuilder());

            Assert.Equal(GetThirdCardView(), result);
        }

        [Fact]
        public void ShouldReturnRenderedCard4()
        {
            var renderer = new CardComponent();
            var result = renderer.AddHtml(GetFourthCard(), new StringBuilder());

            Assert.Equal(GetFourthCardView(), result);
        }
    }
}
