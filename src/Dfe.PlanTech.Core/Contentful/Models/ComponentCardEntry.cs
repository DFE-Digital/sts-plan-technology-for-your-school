using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentCardEntry: TransformableEntry<ComponentCardEntry, CmsComponentCardDto>
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; } = null!;
    public string? Meta { get; init; } = null!;
    public Asset? Image { get; init; } = null!;
    public string? ImageAlt { get; init; } = null!;
    public string? Uri { get; init; } = null!;

    protected override Func<ComponentCardEntry, CmsComponentCardDto> Constructor => entry => new(entry);
}
