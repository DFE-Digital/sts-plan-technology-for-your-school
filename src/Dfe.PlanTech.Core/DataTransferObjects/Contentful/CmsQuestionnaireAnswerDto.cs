using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsQuestionnaireAnswerDto: CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string Maturity { get; set; }
    public CmsQuestionnaireQuestionDto? NextQuestion { get; set; }

    public CmsQuestionnaireAnswerDto(QuestionnaireAnswerEntry answerEntry)
    {
        Id = answerEntry.Id;
        Text = answerEntry.Text;
        Maturity = answerEntry.Maturity;
        NextQuestion = answerEntry.NextQuestion?.AsDto();
    }
}
