using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class PageEntry : ContentfulEntry<CmsPageDto>
    {
        public string InternalName { get; init; } = null!;

        public string Slug { get; init; } = null!;

        public bool DisplayBackButton { get; init; }

        public bool DisplayHomeButton { get; init; }

        public bool DisplayTopicTitle { get; init; }

        public bool DisplayOrganisationName { get; init; }

        public bool RequiresAuthorisation { get; init; } = true;

        public string? SectionTitle { get; set; }

        public List<ContentfulEntry> BeforeTitleContent { get; init; } = [];

        public TitleEntry? Title { get; init; }

        public string? OrganisationName { get; set; }

        public List<ContentfulEntry> Content { get; init; } = [];

        protected override CmsPageDto CreateDto()
        {
            throw new NotImplementedException();
        }
    }
}
