using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsQuestionDto : CmsEntryDto
    {
        public CmsQuestionDto(QuestionEntry questionEntry)
        {
            Slug = questionEntry.Slug;
            Text = questionEntry.Text;
            HelpText = questionEntry.HelpText;
            Answers = questionEntry.Answers.Select(a => a.AsDto());
        }

        public string Slug { get; set; } = null!;

        public string Text { get; set; } = null!;

        public string? HelpText { get; set; }

        public List<CmsAnswerDto> Answers { get; set; } = new();
    }
}
