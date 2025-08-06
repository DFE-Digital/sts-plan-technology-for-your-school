using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsFieldDto
{
    protected CmsFieldDto BuildContentDto(ContentfulEntry contentComponent)
    {
        if (contentComponent is IDtoTransformableField field)
        {
            return field.AsDtoInternal();
        }

        throw new ArgumentException($"{nameof(ContentfulField)} cannot be transformed to a DTO.");
    }
}
