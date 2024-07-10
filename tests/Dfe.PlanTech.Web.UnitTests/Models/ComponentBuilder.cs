using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public class ComponentBuilder : IComponentBuilder
    {
        public Category BuildCategory()
        {
            return new Category()
            {
                Header = new Header { Text = "Category" },
                Sections = BuildSections(),
            };
        }

        public TextBody BuildTextBody()
        {
            return new TextBody
            {
                RichText = BuildRichContent()
            };
        }

        public ComponentDropDown BuildDropDownComponent()
        {
            return new ComponentDropDown
            {
                Title = "Dropdown",
                Content = BuildRichContent()
            };
        }

        public ButtonWithLink BuildButtonWithLink()
        {
            return new ButtonWithLink
            {
                Button = BuildButton(),
                Href = "/FakeLink"
            };
        }

        public ButtonWithEntryReference BuildButtonWithEntryReference()
        => new()
        {
            Button = BuildButton(),
            LinkToEntry = BuildButton()
        };

        public InsetText BuildInsetText() => new()
        {
            Text = "Inset Text"
        };

        public List<Section> BuildSections()
        =>
        [
            new Section
            {
                Name = "Section",
                Questions = BuildQuestions(),
            }
        ];

        public static RecommendationsViewModel BuildRecommendationViewModel()
        => new()
        {
            Intro = BuildRecommendationIntro("intro"),
            Chunks = [BuildRecommendationChunk("First", "Title one"), BuildRecommendationChunk("Second", "Title two"), BuildRecommendationChunk("Third", "Title three")]
        };

        public static RecommendationIntro BuildRecommendationIntro(string header) => new() { Header = new Header() { Text = header } };

        public static RecommendationChunk BuildRecommendationChunk(string header, string title = "Title") => new() { Header = new Header() { Text = header } };

        private static List<Question> BuildQuestions()
        {
            return
            [
                new Question
                {
                    Text = "Question Text",
                    HelpText = "Help Text",
                    Answers = BuildAnswers(),
                }
            ];
        }

        private static List<Answer> BuildAnswers()
        {
            return
            [
                new Answer
                {
                    Text = "Answer"
                }
            ];
        }

        private static RichTextContent BuildRichContent()
        {
            return new RichTextContent { Value = "Content" };
        }

        private static RichTextContent BuildRichContent(string text)
        {
            return new RichTextContent { Value = text };
        }

        private static Button BuildButton()
        {
            return new Button
            {
                Value = "Submit",
                IsStartButton = false,
            };
        }

        private static Page BuildPage(string? param = null)
        => new()
        {
            InternalName = "Internal Name",
            Slug = "testing-page",
            SectionTitle = "Section Title",
            Title = BuildTitle(),
            Content = []
        };

        private static Title BuildTitle(string text = "Testing Title")
        => new()
        {
            Text = text
        };

        public WarningComponent BuildWarningComponent(string text)
        {
            return new WarningComponent
            {
                Text = new TextBody
                {
                    RichText = BuildRichContent(text)
                }
            };
        }
    }
}