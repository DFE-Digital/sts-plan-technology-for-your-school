using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public abstract class TransformableEntry<TSelf, TDto> : Entry<ContentComponent>, IDtoTransformable<TDto>
    where TSelf : TransformableEntry<TSelf, TDto>
    where TDto : CmsEntryDto
{
    private readonly Func<TSelf, TDto> _constructor;

    protected TransformableEntry(Func<TSelf, TDto> constructor)
    {
        _constructor = constructor;
    }

    CmsEntryDto IDtoTransformable.AsDtoInternal() => _constructor((TSelf)this);
    TDto IDtoTransformable<TDto>.AsDtoInternal() => _constructor((TSelf)this);

    public TDto AsDto() => _constructor((TSelf)this);
}
