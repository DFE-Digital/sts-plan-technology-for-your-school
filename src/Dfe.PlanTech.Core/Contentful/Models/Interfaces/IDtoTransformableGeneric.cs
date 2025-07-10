using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces
{
    public interface IDtoTransformable<TDto> : IDtoTransformable
        where TDto : CmsEntryDto
    {
        public new TDto AsDtoInternal();
    }
}
