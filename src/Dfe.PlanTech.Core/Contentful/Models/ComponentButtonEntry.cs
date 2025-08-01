using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonEntry: TransformableEntry<ComponentButtonEntry, CmsComponentButtonDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Value { get; init; } = null!;
    public bool IsStartButton { get; init; }

    public ComponentButtonEntry() : base(entry => new CmsComponentButtonDto(entry)) { }

}
