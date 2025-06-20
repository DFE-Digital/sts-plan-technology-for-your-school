namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsAnswerDto
    {
        public string Text { get; init; } = null!;

        public CmsQuestionDto? NextQuestion { get; init; }

        public string Maturity { get; init; } = null!;
    }
}
