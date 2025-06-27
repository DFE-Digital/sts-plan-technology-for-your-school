namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsEntrySystemDetailsDto
    {
        public string? Id { get; set; }

        public string? Type { get; set; }

        public string? ContentType { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? Revision { get; set; }

        public CmsEntryLinkDto? Space { get; set; }

        public CmsEntryLinkDto? Environment { get; set; }

        public CmsEntryLinkDto? ContentTypeLink { get; set; }

        public CmsEntryLinkDto? CreatedBy { get; set; }

        public CmsEntryLinkDto? UpdatedBy { get; set; }
    }
}
