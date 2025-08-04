using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentWarningEntry : TransformableEntry<ComponentWarningEntry, CmsComponentWarningDto>
{
    public string InternalName { get; set; } = null!;
    public ComponentTextBodyEntry Text { get; init; } = null!;

    protected override Func<ComponentWarningEntry, CmsComponentWarningDto> Constructor => entry => new(entry);
}
