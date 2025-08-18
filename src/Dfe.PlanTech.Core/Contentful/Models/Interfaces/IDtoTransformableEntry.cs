using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces;

public interface IDtoTransformableEntry
{
    public SystemDetails Sys { get; set; }
    public string Description { get; set; }

    public CmsEntryDto AsDtoInternal();
}
