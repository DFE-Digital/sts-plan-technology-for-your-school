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

        public RecommendationPage BuildRecommendationsPage(Maturity maturity = Maturity.Unknown)
        => new()
        {
            Page = BuildPage(),
            DisplayName = $"Testing Recommendation - {maturity}",
            InternalName = $"testing-recommendation-{maturity}",
            Maturity = maturity,
        };

        public InsetText BuildInsetText() => new()
        {
            Text = "Inset Text"
        };

        public List<Section> BuildSections()
        => new()
        {
            new Section
            {
                Name = "Section",
                Questions = BuildQuestions(),
                Recommendations =new() {
                    BuildRecommendationsPage(Maturity.Low),
                    BuildRecommendationsPage(Maturity.Medium),
                    BuildRecommendationsPage(Maturity.High)
                }
            }
        };

        private static List<Question> BuildQuestions()
        {
            return new()
            {
                new Question
                {
                    Text = "Question Text",
                    HelpText = "Help Text",
                    Answers = BuildAnswers(),
                }
            };
        }

        private static List<Answer> BuildAnswers()
        {
            return new()
            {
                new Answer
                {
                    Text = "Answer"
                }
            };
        }

        private static IContentComponent[] BuildContent()
        {
            return new IContentComponent[]
            {
                BuildButton()
            };
        }

        private static RichTextContent BuildRichContent()
        {
            return new RichTextContent { Value = "Content" };
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
            Content = new()
        };

        private static Title BuildTitle(string text = "Testing Title")
        => new()
        {
            Text = text
        };
    }
}