namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsQuestionDto : CmsEntryDto
    {
        public string Slug { get; set; } = null!;

        public string Text { get; set; } = null!;

        public string? HelpText { get; set; }

        public List<CmsAnswerDto> Answers { get; set; } = new();
    }
}
