using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public class ComponentBuilder : IComponentBuilder
    {
        public Category BuildCategory()
        {
            return new Category
            {
                Header = new Header { Text = "Category" },
                Content = BuildContent(),
                Sections = BuildSections()
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

        private static ISection[] BuildSections()
        {
            return new ISection[]
            {
                new Section
                {
                    Name = "Section",
                    Questions = BuildQuestion(),
                }
            };
        }

        private static Question[] BuildQuestion()
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

        private static Answer[] BuildAnswer()
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
    }
}