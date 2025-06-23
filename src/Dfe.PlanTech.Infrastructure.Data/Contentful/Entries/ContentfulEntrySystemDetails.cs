namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class ContentfulEntrySystemDetails
    {
        public string? Id { get; set; }

        public string? Type { get; set; }

        public string? ContentType { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? Revision { get; set; }

        public ContentfulEntryLink? Space { get; set; }

        public ContentfulEntryLink? Environment { get; set; }

        public ContentfulEntryLink? ContentTypeLink { get; set; }

        public ContentfulEntryLink? CreatedBy { get; set; }

        public ContentfulEntryLink? UpdatedBy { get; set; }

        public CmsEntrySystemDetailsDto ToDto()
        {
            return new CmsEntrySystemDetailsDto
            {
                Id = Id,
                Type = Type,
                ContentType = ContentType,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Revision = Revision,
                Space = Space?.ToDto(),
                Environment = Environment?.ToDto(),
                ContentTypeLink = ContentTypeLink?.ToDto(),
                CreatedBy = CreatedBy?.ToDto(),
                UpdatedBy = UpdatedBy?.ToDto()
            };
        }
    }
}
