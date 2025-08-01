using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTitleEntry: TransformableEntry<ComponentTitleEntry, CmsComponentTitleDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;

    public ComponentTitleEntry() : base(entry => new CmsComponentTitleDto(entry)) {}
}
