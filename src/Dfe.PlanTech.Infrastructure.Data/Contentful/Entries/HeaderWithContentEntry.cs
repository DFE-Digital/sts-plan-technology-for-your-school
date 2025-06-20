namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class HeaderWithContentEntry : ContentfulEntry
    {
        public string HeaderText { get; init; } = null!;

        public List<ContentfulEntry> Content { get; init; } = null!;

        public string LinkText { get; init; } = null!;

        public string SlugifiedLinkText { get; init; } = null!;
    }
}
