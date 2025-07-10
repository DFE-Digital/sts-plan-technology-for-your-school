using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonEntry: TransformableEntry<ComponentButtonEntry, CmsDto>
{
    public ComponentButtonEntry() : base(entry => new CmsDto(entry)) {}

{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public string Value { get; init; } = null!;
    public bool IsStartButton { get; init; }
}
