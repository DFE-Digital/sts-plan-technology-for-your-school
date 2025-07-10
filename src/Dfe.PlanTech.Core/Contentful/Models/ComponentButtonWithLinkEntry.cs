using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithLinkEntry: TransformableEntry<ComponentButtonWithLinkEntry, CmsDto>
{
    public ComponentButtonWithLinkEntry() : base(entry => new CmsDto(entry)) {}

{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public string Href { get; init; } = null!;
}
