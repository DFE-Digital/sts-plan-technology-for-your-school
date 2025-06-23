using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects;

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

        protected override CmsRecommendationDto CreateDto()
        {
            return new CmsRecommendationDto
            {
                Slug = Slug,
                Maturity = Maturity,
                Content = Content.Select(c => c.ToDto()).ToList(),
                HeaderText = HeaderText,
                LinkText = LinkText,
                SlugifiedLinkText = SlugifiedLinkText
            };
        }
    }
}
