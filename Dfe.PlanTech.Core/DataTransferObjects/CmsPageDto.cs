namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsPageDto
    {
        public string InternalName { get; init; } = null!;

        public string Slug { get; init; } = null!;

        public bool DisplayBackButton { get; init; }

        public bool DisplayHomeButton { get; init; }

        public bool DisplayTopicTitle { get; init; }

        public bool DisplayOrganisationName { get; init; }

        public bool RequiresAuthorisation { get; init; } = true;

        public string? SectionTitle { get; set; }

        public List<CmsEntryDto> BeforeTitleContent { get; init; } = [];

        public CmsTitleDto? Title { get; init; }

        public string? OrganisationName { get; set; }

        public List<CmsEntryDto> Content { get; init; } = [];
    }
}
