using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering.Models
{
    public class CustomAttachmentModel
    {
        public string? InternalName { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public string? Title { get; set; }
        public string? Uri { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? FileExtension { get; set; }

        public CustomAttachmentModel(RichTextContentDataEntry content)
        {
            var contentType = content?.Asset?.File.ContentType;
            var fileExtension = contentType?.Split('/')[^1].ToLower();

            if (fileExtension == FileExtensionConstants.XLSXSPREADSHEET)
            {
                fileExtension = FileExtensionConstants.XLSX;
            }

            InternalName = content?.InternalName ?? string.Empty;
            ContentType = contentType ?? string.Empty;
            Size = content?.Asset?.File?.Details?.Size / 1024 ?? 0;
            Title = content?.Title;
            Uri = content?.Asset?.File.Url ?? string.Empty;
            UpdatedAt = content?.Asset?.SystemProperties.UpdatedAt;
            FileExtension = fileExtension ?? string.Empty;
        }
    }
}
