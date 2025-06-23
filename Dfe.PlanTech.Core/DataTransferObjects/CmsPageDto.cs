namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsPageDto : CmsEntryDto
    {
        public string InternalName { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public bool DisplayBackButton { get; set; }

        public bool DisplayHomeButton { get; set; }

        public bool DisplayTopicTitle { get; set; }

        public bool DisplayOrganisationName { get; set; }

        public bool RequiresAuthorisation { get; set; } = true;

        public string? SectionTitle { get; set; }

        public List<CmsEntryDto> BeforeTitleContent { get; set; } = [];

        public CmsTitleDto? Title { get; set; }

        public string? OrganisationName { get; set; }

        public List<CmsEntryDto> Content { get; set; } = [];
    }
}
