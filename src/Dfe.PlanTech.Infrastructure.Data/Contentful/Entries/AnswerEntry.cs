namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class AnswerEntry : ContentfulEntry
    {
        public string Text { get; init; } = null!;

        public QuestionEntry? NextQuestion { get; init; }

        public string Maturity { get; init; } = null!;
    }
}
