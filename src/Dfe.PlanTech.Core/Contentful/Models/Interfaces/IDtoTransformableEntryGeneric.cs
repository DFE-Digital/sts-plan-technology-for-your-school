using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces
{
    public interface IDtoTransformableEntry<TDto> : IDtoTransformableEntry
        where TDto : CmsEntryDto
    {
        public new TDto AsDtoInternal();
    }
}
