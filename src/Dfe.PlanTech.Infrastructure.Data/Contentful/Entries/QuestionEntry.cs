namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class QuestionEntry : ContentfulEntry
    {
        public string Slug { get; set; } = null!;

        public string Text { get; init; } = null!;

        public string? HelpText { get; init; }

        public List<AnswerEntry> Answers { get; init; } = new();
    }
}
