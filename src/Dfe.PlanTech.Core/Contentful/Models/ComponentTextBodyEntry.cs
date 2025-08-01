using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTextBodyEntry: TransformableEntry<ComponentTextBodyEntry, CmsComponentTextBodyDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public RichTextContent RichText { get; init; } = null!;

    public ComponentTextBodyEntry() : base(entry => new CmsComponentTextBodyDto(entry)) {}
}
