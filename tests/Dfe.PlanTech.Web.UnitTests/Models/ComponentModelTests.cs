using Dfe.PlanTech.Domain.Submissions.Models;
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
            Assert.NotNull(actual.Content);
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
        [InlineData("Save a back-up...", "save-a-back-up")]
        [InlineData("This is a string with loads of spaces at the end        ", "this-is-a-string-with-loads-of-spaces-at-the-end")]
        [InlineData("This is a string with loads of spaces at the end        and-this", "this-is-a-string-with-loads-of-spaces-at-the-end--------and-this")]
        [InlineData(" spaces either side     ", "spaces-either-side")]
        [InlineData(@"Line
separator
character
", "line-separator-character")]
        public void Slugify_Should_Slugify_Strings(string linkText, string expectedResult)
        {
            var recommendationChunk = ComponentBuilder.BuildRecommendationChunk(linkText);

            Assert.Equal(expectedResult, recommendationChunk.SlugifiedLinkText);
        }

        [Theory]
        [InlineData("Random test Topic")]
        [InlineData("Y867as ()&ycj Cool Thing")]
        public void RecommendationIntro_Should_Return_Correct_Header_And_Title(string header)
        {
            var recommendationIntro = ComponentBuilder.BuildRecommendationIntro(header);

            Assert.Equal(header, recommendationIntro.Header.Text);
            Assert.Equal(header, recommendationIntro.HeaderText);
        }

        [Theory]
        [InlineData("Random test Topic", "overview")]
        [InlineData("Pasdw!345      dsdoiu()=2 Yo ", "overview")]
        public void RecommendationIntro_Should_Return_Correct_LinkText(string header, string expectedResult)
        {
            var recommendationIntro = ComponentBuilder.BuildRecommendationIntro(header);

            Assert.Equal("Overview", recommendationIntro.LinkText);
            Assert.Equal(expectedResult, recommendationIntro.SlugifiedLinkText);
        }

        [Theory]
        [InlineData("Random test Topic", "random-test-topic")]
        [InlineData("Y867as ()&ycj Cool Thing", "y867as-ycj-cool-thing")]
        public void RecommendationChunk_Should_Return_Correct_Header_Title_And_LinkText(string header, string expectedResult)
        {
            var recommendationChunk = ComponentBuilder.BuildRecommendationChunk(header);

            Assert.Equal(header, recommendationChunk.HeaderText);
            Assert.Equal(header, recommendationChunk.LinkText);
            Assert.Equal(expectedResult, recommendationChunk.SlugifiedLinkText);
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
        public void Recommendation_View_Model_Should_Return_SubmissionResponses()
        {
            var submission = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer { QuestionRef = "Q1", QuestionText = "First question", AnswerRef = "A1", AnswerText = "First answer", DateCreated = new DateTime() },
                new QuestionWithAnswer { QuestionRef = "Q2", QuestionText = "Second question", AnswerRef = "A2", AnswerText = "Second answer", DateCreated = new DateTime() },
                new QuestionWithAnswer { QuestionRef = "Q3", QuestionText = "Third question", AnswerRef = "A3", AnswerText = "Third answer", DateCreated = new DateTime() },
                new QuestionWithAnswer { QuestionRef = "Q4", QuestionText = "Fourth question", AnswerRef = "A4", AnswerText = "Fourth answer", DateCreated = new DateTime() },
            };

            var recommendationViewModel = ComponentBuilder.BuildRecommendationViewModel(submission);

            var responses = recommendationViewModel.SubmissionResponses.ToList();

            Assert.Equal(4, responses.Count());
            Assert.Contains(submission[3], responses);
            Assert.Equal("Q1", responses[0].QuestionRef);
        }
    }
}
