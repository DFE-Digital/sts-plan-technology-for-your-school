using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces
{
    public interface IDtoTransformable
    {
        public CmsEntryDto AsDtoInternal();
    }
}
