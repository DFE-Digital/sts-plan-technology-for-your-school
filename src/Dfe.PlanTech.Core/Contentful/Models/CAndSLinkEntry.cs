using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class CAndSLinkEntry: TransformableEntry<CAndSLinkEntry, CmsCAndSLinkDto>
{
    public CAndSLinkEntry() : base(entry => new CmsCAndSLinkDto(entry)) {}

{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string LinkText { get; set; } = null!;

    public CmsCAndSLinkDto AsDto() => new(this);
}
