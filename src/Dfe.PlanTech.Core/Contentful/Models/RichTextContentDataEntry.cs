using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RichTextContentDataEntry: TransformableEntry<RichTextContentDataEntry, CmsRichTextContentDataDto>, IHasUri
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;
    public IReadOnlyList<RichTextContentDataEntry> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;
    public RichTextContent RichText { get; init; } = null!;

    public RichTextContentDataEntry() : base(entry => new CmsRichTextContentDataDto(entry)) {}
}
