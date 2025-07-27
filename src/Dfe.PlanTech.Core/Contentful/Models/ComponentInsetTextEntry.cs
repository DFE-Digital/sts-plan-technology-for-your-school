using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentInsetTextEntry: TransformableEntry<ComponentInsetTextEntry, CmsComponentInsetTextDto>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;

    public ComponentInsetTextEntry() : base(entry => new CmsComponentInsetTextDto(entry)) {}
}
