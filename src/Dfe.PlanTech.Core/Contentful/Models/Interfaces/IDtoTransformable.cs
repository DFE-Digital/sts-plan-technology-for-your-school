using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces;

public interface IDtoTransformable
{
    public SystemProperties SystemProperties { get; set; }

    public CmsEntryDto AsDtoInternal();
}
