using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public abstract class TransformableEntry<TSelf, TDto> : ContentComponent, IDtoTransformable<TDto>
    where TSelf : TransformableEntry<TSelf, TDto>
    where TDto : CmsEntryDto
{
    public string Id => Sys.Id;

    protected abstract Func<TSelf, TDto> Constructor { get; }

    protected TransformableEntry() { }

    CmsEntryDto IDtoTransformable.AsDtoInternal() => AsDto();
    TDto IDtoTransformable<TDto>.AsDtoInternal() => AsDto();

    public TDto AsDto()
    {
        if (Constructor is null)
        {
            throw new InvalidOperationException($"{GetType().Name} was deserialized without a transformation function. You must call SetTransformConstructor first.");
        }

        return Constructor((TSelf)this);
    }
}
