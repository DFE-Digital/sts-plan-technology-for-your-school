using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsQuestionnaireQuestionDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public List<CmsQuestionnaireAnswerDto> Answers { get; set; } = [];
        public string? HelpText { get; set; }
        public string Slug { get; set; } = null!;
        public string Text { get; set; } = null!;

        public CmsQuestionnaireQuestionDto(QuestionnaireQuestionEntry questionEntry)
        {
            Id = questionEntry.Id;
            Answers = questionEntry.Answers.Select(a => a.AsDto()).ToList();
            HelpText = questionEntry.HelpText;
            Slug = questionEntry.Slug;
            Text = questionEntry.Text;
        }
    }
}
