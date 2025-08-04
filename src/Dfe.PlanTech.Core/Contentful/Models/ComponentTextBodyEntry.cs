using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTextBodyEntry: TransformableEntry<ComponentTextBodyEntry, CmsComponentTextBodyDto>
{
    public string InternalName { get; set; } = null!;
    public RichTextContentEntry RichText { get; init; } = null!;

    protected override Func<ComponentTextBodyEntry, CmsComponentTextBodyDto> Constructor => entry => new(entry);
}
