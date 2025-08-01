using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentWarningEntry: TransformableEntry<ComponentWarningEntry, CmsComponentWarningDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentTextBodyEntry Text { get; init; } = null!;

    public ComponentWarningEntry() : base(entry => new CmsComponentWarningDto(entry)) {}
}
