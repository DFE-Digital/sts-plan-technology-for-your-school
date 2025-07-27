using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class MissingComponentEntry : TransformableEntry<MissingComponentEntry, CmsMissingComponentDto>
{
    public MissingComponentEntry() : base(entry => new CmsMissingComponentDto()) { }
}
