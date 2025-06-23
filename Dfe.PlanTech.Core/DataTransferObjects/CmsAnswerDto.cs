namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsAnswerDto : CmsEntryDto
    {
        public string Text { get; set; } = null!;

        public CmsQuestionDto? NextQuestion { get; set; }

        public string Maturity { get; set; } = null!;
    }
}
