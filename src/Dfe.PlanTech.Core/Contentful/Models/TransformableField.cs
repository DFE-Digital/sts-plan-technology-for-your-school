using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public abstract class TransformableField<TSelf, TDto> : ContentfulField, IDtoTransformableField<TDto>
    where TSelf : TransformableField<TSelf, TDto>
    where TDto : CmsFieldDto
{
    protected abstract Func<TSelf, TDto> Constructor { get; }

    protected TransformableField() { }

    CmsFieldDto IDtoTransformableField.AsDtoInternal() => AsDto();
    TDto IDtoTransformableField<TDto>.AsDtoInternal() => AsDto();

    public TDto AsDto()
    {
        if (Constructor is null)
        {
            throw new InvalidOperationException($"{GetType().Name} was deserialized without a transformation function. You must call SetTransformConstructor first.");
        }

        return Constructor((TSelf)this);
    }
}
