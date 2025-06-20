namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsQuestionDto
    {
        public string Slug { get; set; } = null!;

        public string Text { get; init; } = null!;

        public string? HelpText { get; init; }

        public List<CmsAnswerDto> Answers { get; init; } = new();
    }
}
