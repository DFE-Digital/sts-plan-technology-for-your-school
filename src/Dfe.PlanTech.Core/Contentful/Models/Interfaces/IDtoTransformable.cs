using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces;

public interface IDtoTransformable
{
    public SystemProperties Sys { get; set; }
    public string Description { get; set; }

    public CmsEntryDto AsDtoInternal();
}
