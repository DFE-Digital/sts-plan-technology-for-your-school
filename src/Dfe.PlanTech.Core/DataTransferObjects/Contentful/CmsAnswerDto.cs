using Dfe.PlanTech.Core.Content.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsAnswerDto: CmsEntryDto
    {
        public CmsAnswerDto(QuestionnaireAnswerEntry answerEntry)
        {
            Sys.Id = answerEntry.Sys.Id;
            Text = answerEntry.Text;
            Maturity = answerEntry.Maturity;
            NextQuestion = answerEntry.NextQuestion?.AsDto();
        }

        public string Text { get; set; }

        public CmsQuestionDto? NextQuestion { get; set; }

        public string Maturity { get; set; }
    }
}
