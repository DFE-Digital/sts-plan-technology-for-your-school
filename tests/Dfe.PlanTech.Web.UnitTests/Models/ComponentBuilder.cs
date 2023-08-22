using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public class ComponentBuilder : IComponentBuilder
    {
        
        private IGetSubmissionStatusesQuery _submissionStatusesQuery;
        private ILogger<Category> _logger;
        
            
        public Category BuildCategory()
        {
            _submissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
            _logger = Substitute.For<ILogger<Category>>();
            
            return new Category(_logger, _submissionStatusesQuery)
            {
                Header = new Header { Text = "Category" },
                Content = BuildContent(),
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

        public ISection[] BuildSections()
         => new ISection[]
            {
                new Section
                {
                    Name = "Section",
                    Questions = BuildQuestion(),
                    Recommendations = new RecommendationPage[] {
                        BuildRecommendationsPage(Maturity.Low),
                        BuildRecommendationsPage(Maturity.Medium),
                        BuildRecommendationsPage(Maturity.High)
                    }
                }
            };

        private Question[] BuildQuestion()
        {
            return new Question[]
            {
                new Question
                {
                    Text = "Question Text",
                    HelpText = "Help Text",
                    Answers = BuildAnswer(),
                }
            };
        }

        private Answer[] BuildAnswer()
        {
            return new Answer[]
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

        private static IDictionary<string, string> GetSectionStatuses()
        {
            return new Dictionary<string, string> { { "3XQEHYfvEQkQwdrihDGagJ", "Completed" } };
        }

        private static Page BuildPage(string? param = null)
        => new()
        {
            InternalName = "Internal Name",
            Slug = "testing-page",
            SectionTitle = "Section Title",
            Param = param,
            Title = BuildTitle(),
            Content = Array.Empty<IContentComponent>()
        };

        private static Title BuildTitle(string text = "Testing Title")
        => new()
        {
            Text = text
        };
    }
}