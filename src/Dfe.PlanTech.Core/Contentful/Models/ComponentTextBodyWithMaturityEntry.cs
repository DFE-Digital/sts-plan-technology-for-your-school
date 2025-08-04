using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTextBodyWithMaturityEntry: TransformableEntry<ComponentTextBodyWithMaturityEntry, CmsComponentTextBodyWithMaturityDto>
{
    public string InternalName { get; set; } = null!;
    public RichTextContentEntry TextBody { get; init; } = null!;
    public string Maturity { get; set; } = null!;

    protected override Func<ComponentTextBodyWithMaturityEntry, CmsComponentTextBodyWithMaturityDto> Constructor => entry => new(entry);
}
