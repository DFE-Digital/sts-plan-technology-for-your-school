namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class SectionEntry : ContentfulEntry
    {
        public string Name { get; init; } = null!;

        public PageEntry? InterstitialPage { get; init; }

        public List<QuestionEntry> Questions { get; init; } = new();

        public string FirstQuestionSysId => Questions
            .Select(question => question.Sys.Id)
            .FirstOrDefault() ?? "";
    }
}
