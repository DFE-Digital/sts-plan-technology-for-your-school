using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonEntry: TransformableEntry<ComponentButtonEntry, CmsComponentButtonDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public string Value { get; init; } = null!;
    public bool IsStartButton { get; init; }

    public ComponentButtonEntry() : base(entry => new CmsComponentButtonDto(entry)) { }

}
