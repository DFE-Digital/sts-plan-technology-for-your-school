using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsQuestionDto : CmsEntryDto
    {
        public string Slug { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string? HelpText { get; set; }
        public List<CmsQuestionnaireAnswerDto> Answers { get; set; } = [];

        public CmsQuestionDto(QuestionnaireQuestionEntry questionEntry)
        {
            Slug = questionEntry.Slug;
            Text = questionEntry.Text;
            HelpText = questionEntry.HelpText;
            Answers = questionEntry.Answers.Select(a => a.AsDto()).ToList();
        }
    }
}
