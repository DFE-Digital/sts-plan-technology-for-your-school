using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class QuestionEntry : ContentfulEntry<CmsQuestionDto>
    {
        public string Slug { get; set; } = null!;

        public string Text { get; init; } = null!;

        public string? HelpText { get; init; }

        public List<AnswerEntry> Answers { get; init; } = new();

        protected override CmsQuestionDto CreateDto()
        {
            return new CmsQuestionDto
            {
                Slug = Slug,
                Text = Text,
                HelpText = HelpText,
                Answers = Answers.Select(a => a.ToDto()).ToList(),
            };
        }
    }
}
