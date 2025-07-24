namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsCustomAttachmentDto
    {
        public string? InternalName { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public string? Title { get; set; }
        public string? Uri { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? FileExtension { get; set; }
    }
}
