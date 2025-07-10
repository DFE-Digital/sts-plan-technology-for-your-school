
using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentWarningEntry: TransformableEntry<ComponentWarningEntry, CmsComponentWarningDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public ComponentTextBodyEntry Text { get; init; } = null!;

    public ComponentWarningEntry() : base(entry => new CmsComponentWarningDto(entry)) {}
}
