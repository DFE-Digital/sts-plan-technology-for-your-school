using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public class ComponentModelTests
    {
        private readonly ComponentBuilder _componentBuilder;

        public ComponentModelTests()
        {
            _componentBuilder = new ComponentBuilder();
        }

        [Fact]
        public void Should_Render_Button_Component()
        {
            var actual = _componentBuilder.BuildButtonWithLink();
            Assert.NotNull(actual);
            Assert.Equal("Submit", actual.Button.Value);
            Assert.False(actual.Button.IsStartButton);
            Assert.Equal("/FakeLink", actual.Href);
        }

        [Fact]
        public void Should_Render_Dropdown_Component()
        {
            var actual = _componentBuilder.BuildDropDownComponent();
            Assert.NotNull(actual);
            Assert.Equal("Dropdown", actual.Title);
            Assert.Equal("Content", actual.Content.Value);
        }

        [Fact]
        public void Should_Render_TextBody_Component()
        {
            var actual = _componentBuilder.BuildTextBody();
            Assert.NotNull(actual);
            Assert.Equal("Content", actual.RichText.Value);
        }

        [Fact]
        public void Should_Render_Category_Component()
        {
            var actual = _componentBuilder.BuildCategory();
            Assert.NotNull(actual);
            Assert.Equal("Category", actual.Header.Text);
            Assert.Equal("Section", actual.Sections[0].Name);
            Assert.NotNull(actual.Sections[0].Questions);
            Assert.Equal("Question Text", actual.Sections[0].Questions[0].Text);
            Assert.Equal("Help Text", actual.Sections[0].Questions[0].HelpText);
            Assert.NotNull(actual.Sections[0].Questions[0].Answers[0]);
            Assert.Equal("Answer", actual.Sections[0].Questions[0].Answers[0].Text);
            Assert.Equal(0, actual.Completed);
        }


        [Fact]
        public void Should_Render_ButtonWithEntryReference()
        {
            var actual = _componentBuilder.BuildButtonWithEntryReference();
            Assert.NotNull(actual);
            Assert.Equal("Submit", actual.Button.Value);
            Assert.False(actual.Button.IsStartButton);
            Assert.NotNull(actual.LinkToEntry);
        }

        [Fact]
        public void Should_Render_InsetText()
        {
            var actual = _componentBuilder.BuildInsetText();

            Assert.NotNull(actual);
            Assert.Equal("Inset Text", actual.Text);
        }

        [Theory]
        [InlineData("Random test Topic", "random-test-topic")]
        [InlineData("Y867as ()&ycj Cool Thing", "y867as-ycj-cool-thing")]
        public void Slugify_Should_Slugify_Strings(string header, string expectedResult)
        {
            var recommendationChunk = ComponentBuilder.BuildRecommendationChunk(header);

            Assert.Equal(expectedResult, recommendationChunk.SlugifiedHeader);
        }

        [Theory]
        [InlineData("Random test Topic", "random-test-topic")]
        [InlineData("Y867as ()&ycj Cool Thing", "y867as-ycj-cool-thing")]
        public void RecommendationIntro_Should_Return_Correct_Header_And_Title(string header, string expectedResult)
        {
            var recommendationIntro = ComponentBuilder.BuildRecommendationIntro(header);

            Assert.Equal(header, recommendationIntro.Title);
            Assert.Equal(expectedResult, recommendationIntro.SlugifiedHeader);
        }

        [Fact]
        public void RecommendationViewModel_Should_Return_All_Content()
        {
            var recommendationViewModel = ComponentBuilder.BuildRecommendationViewModel();

            var allContent = recommendationViewModel.AllContent.ToList();

            Assert.Contains(recommendationViewModel.Intro, allContent);

            foreach (var chunk in recommendationViewModel.Chunks)
            {
                Assert.Contains(chunk, allContent);
            }
        }

        [Fact]
        public void Should_render_warning_component()
        {
            var text = "test text";
            var actual = _componentBuilder.BuildWarningComponent(text);
            Assert.NotNull(actual);
            Assert.Equal("test text", actual.Text.RichText.Value);
        }
    }
}
