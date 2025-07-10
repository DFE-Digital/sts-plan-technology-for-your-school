using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RichTextFieldEntry: TransformableEntry<RichTextFieldEntry, CmsRichTextFieldDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;
    public IReadOnlyList<RichTextFieldEntry> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;
    public RichTextContent RichText { get; init; } = null!;

    public RichTextFieldEntry() : base(entry => new CmsRichTextFieldDto(entry)) {}
}
