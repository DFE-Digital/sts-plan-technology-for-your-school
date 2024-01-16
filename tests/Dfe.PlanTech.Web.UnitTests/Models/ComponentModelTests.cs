using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public class ComponentModelTests
    {
        private IComponentBuilder _componentBuilder;

        public ComponentModelTests()
        {
            _componentBuilder = new ComponentBuilder();
        }

        [Fact]
        public void Should_render_button_component()
        {
            var actual = _componentBuilder.BuildButtonWithLink();
            Assert.True(actual != null);
            Assert.Equal("Submit", actual.Button.Value);
            Assert.False(actual.Button.IsStartButton);
            Assert.Equal("/FakeLink", actual.Href);
        }

        [Fact]
        public void Should_render_dropdown_component()
        {
            var actual = _componentBuilder.BuildDropDownComponent();
            Assert.True(actual != null);
            Assert.Equal("Dropdown", actual.Title);
            Assert.Equal("Content", actual.Content.Value);
        }

        [Fact]
        public void Should_render_textbody_component()
        {
            var actual = _componentBuilder.BuildTextBody();
            Assert.True(actual != null);
            Assert.Equal("Content", actual.RichText.Value);
        }

        [Fact]
        public void Should_render_category_component()
        {
            var actual = _componentBuilder.BuildCategory();
            Assert.True(actual != null);
            Assert.Equal("Category", actual.Header.Text);
            Assert.Equal("Section", actual.Sections[0].Name);
            Assert.True(actual.Sections[0].Questions != null);
            Assert.Equal("Question Text", actual.Sections[0].Questions[0].Text);
            Assert.Equal("Help Text", actual.Sections[0].Questions[0].HelpText);
            Assert.True(actual.Sections[0].Questions[0].Answers[0] != null);
            Assert.Equal("Answer", actual.Sections[0].Questions[0].Answers[0].Text);
            Assert.True(actual.Completed == 0);
        }


        [Fact]
        public void Should_Render_ButtonWithEntryReference()
        {
            var actual = _componentBuilder.BuildButtonWithEntryReference();
            Assert.True(actual != null);
            Assert.Equal("Submit", actual.Button.Value);
            Assert.False(actual.Button.IsStartButton);
            Assert.NotNull(actual.LinkToEntry);
        }

        [Fact]
        public void Should_Render_InsetText()
        {
            var actual = _componentBuilder.BuildInsetText();

            Assert.True(actual != null);
            Assert.Equal("Inset Text", actual.Text);
        }

        [Fact]
        public void Should_Render_RecommendationPage()
        {
            var maturity = Maturity.Low;
            var recommendationPage = _componentBuilder.BuildRecommendationsPage(maturity);

            Assert.NotNull(recommendationPage);
            Assert.NotNull(recommendationPage.Page);
            Assert.NotNull(recommendationPage.InternalName);
            Assert.NotNull(recommendationPage.DisplayName);
            Assert.Equal(maturity, recommendationPage.Maturity);
        }

        [Fact]
        public void Section_Should_Return_Correct_Maturity()
        {
            var maturity = Maturity.Low;
            var section = _componentBuilder.BuildSections().First();

            var lowMaturityRecommendation = section.TryGetRecommendationForMaturity(maturity);

            Assert.NotNull(lowMaturityRecommendation);
            Assert.Equal(maturity, lowMaturityRecommendation.Maturity);
        }

        [Fact]
        public void Section_Should_Return_Correct_Maturity_When_Maturity_Is_A_String()
        {
            var maturity = "Low";
            var section = _componentBuilder.BuildSections().First();

            var lowMaturityRecommendation = section.GetRecommendationForMaturity(maturity);

            Assert.NotNull(lowMaturityRecommendation);
            Assert.Equal(Maturity.Low, lowMaturityRecommendation.Maturity);
        }

        [Fact]
        public void Section_Should_Return_Null_When_Maturity_Is_A_String_And_Is_Null()
        {
            string? maturity = null;
            var section = _componentBuilder.BuildSections().First();
            var recommendation = section.GetRecommendationForMaturity(maturity);

            Assert.Null(recommendation);
        }

        [Fact]
        public void Section_Should_Return_Null_If_Maturity_Not_Found()
        {
            var maturity = Maturity.Unknown;
            var section = _componentBuilder.BuildSections().First();

            var unknownMaturityRecommendation = section.TryGetRecommendationForMaturity(maturity);

            Assert.Null(unknownMaturityRecommendation);
        }
    }
}
