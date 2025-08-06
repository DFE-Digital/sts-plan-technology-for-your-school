using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces
{
    public interface IDtoTransformableField<TDto> : IDtoTransformableField
        where TDto : CmsFieldDto
    {
        public new TDto AsDtoInternal();
    }
}
