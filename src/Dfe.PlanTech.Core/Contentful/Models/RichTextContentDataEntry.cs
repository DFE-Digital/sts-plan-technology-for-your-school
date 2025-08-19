using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RichTextContentDataEntry : TransformableEntry<RichTextContentDataEntry, CmsRichTextContentDataDto>, IHasSlug, IHasUri
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public Asset? Asset { get; init; }
    public IReadOnlyList<RichTextContentDataEntry> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;
    public RichTextContentField RichText { get; init; } = null!;

    protected override Func<RichTextContentDataEntry, CmsRichTextContentDataDto> Constructor => entry => new(entry);
}
