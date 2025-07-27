using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentDynamicContentEntry: TransformableEntry<ComponentDynamicContentEntry, CmsComponentDynamicContentDto>
{
    public string InternalName { get; init; } = null!;
    public string DynamicField { get; init; } = null!;

    public ComponentDynamicContentEntry() : base(entry => new CmsComponentDynamicContentDto(entry)) { }
}

