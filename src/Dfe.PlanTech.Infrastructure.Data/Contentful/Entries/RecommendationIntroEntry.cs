using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class RecommendationIntroEntry : ContentfulEntry
    {
        public string Slug { get; init; } = null!;

        public HeaderEntry Header { get; init; } = null!;

        public string Maturity { get; init; } = null!;

        [NotMapped]
        public List<ContentfulEntry> Content { get; init; } = [];

        public string HeaderText => Header.Text;

        public string LinkText => "Overview";

        public string SlugifiedLinkText => _slugifiedLinkText ??= LinkText.Slugify();

        private string? _slugifiedLinkText;
    }
}
