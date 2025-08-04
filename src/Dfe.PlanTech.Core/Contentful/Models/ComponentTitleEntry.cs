using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTitleEntry: TransformableEntry<ComponentTitleEntry, CmsComponentTitleDto>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;

    protected override Func<ComponentTitleEntry, CmsComponentTitleDto> Constructor => entry => new(entry);
}
