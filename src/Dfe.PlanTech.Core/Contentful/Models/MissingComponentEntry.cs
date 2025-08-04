using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class MissingComponentEntry : TransformableEntry<MissingComponentEntry, CmsMissingComponentDto>
{
    protected override Func<MissingComponentEntry, CmsMissingComponentDto> Constructor => entry => new();
}
