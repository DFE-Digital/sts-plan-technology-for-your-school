using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class CAndSLinkEntry : TransformableEntry<CAndSLinkEntry, CmsCAndSLinkDto>
{
    public string InternalName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string LinkText { get; set; } = null!;

    protected override Func<CAndSLinkEntry, CmsCAndSLinkDto> Constructor => entry => new(entry);
}
