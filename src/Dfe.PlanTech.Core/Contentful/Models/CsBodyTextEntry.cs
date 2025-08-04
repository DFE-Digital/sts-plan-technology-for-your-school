using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class CsBodyTextEntry: TransformableEntry<CsBodyTextEntry, CmsCsBodyTextDto>
{
    public string InternalName { get; set; } = null!;
    public RichTextContentEntry RichText { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;

    protected override Func<CsBodyTextEntry, CmsCsBodyTextDto> Constructor => entry => new(entry);
}
