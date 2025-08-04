using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonEntry: TransformableEntry<ComponentButtonEntry, CmsComponentButtonDto>
{
    public string InternalName { get; set; } = null!;
    public string Value { get; init; } = null!;
    public bool IsStartButton { get; init; }

    protected override Func<ComponentButtonEntry, CmsComponentButtonDto> Constructor => entry => new(entry);

}
