using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class CAndSLinkEntry : TransformableEntry<CAndSLinkEntry, CmsCAndSLinkDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string LinkText { get; set; } = null!;

    public CAndSLinkEntry() : base(entry => new CmsCAndSLinkDto(entry)) { }
}
