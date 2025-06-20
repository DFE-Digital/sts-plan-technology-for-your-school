namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class RecommendationChunkEntry : ContentfulEntry
    {
        public string Header { get; init; } = null!;

        public List<ContentfulEntry> Content { get; init; } = [];

        public List<AnswerEntry> Answers { get; init; } = [];

        public CsLinkEntry? CSLink { get; init; }

        public string HeaderText => Header;

        public string LinkText => HeaderText;

        private string? _slugifiedLinkText;

        public string SlugifiedLinkText => _slugifiedLinkText ??= LinkText.Slugify();
    }
}
