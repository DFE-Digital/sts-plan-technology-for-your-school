using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentHeroEntry: TransformableEntry<ComponentHeroEntry, CmsComponentHeroDto>
{
    public string InternalName { get; set; } = null!;
    public IEnumerable<ContentComponent> Content { get; set; } = null!;

    public ComponentHeroEntry() : base(entry => new CmsComponentHeroDto(entry)) {}
}
