using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentHeroEntry: TransformableEntry<ComponentHeroEntry, CmsComponentHeroDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public IEnumerable<Entry<ContentComponent>> Content { get; set; } = null!;

    public ComponentHeroEntry() : base(entry => new CmsComponentHeroDto(entry)) {}
}
