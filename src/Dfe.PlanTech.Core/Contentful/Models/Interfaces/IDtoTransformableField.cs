using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces;

public interface IDtoTransformableField
{
    public CmsFieldDto AsDtoInternal();
}
