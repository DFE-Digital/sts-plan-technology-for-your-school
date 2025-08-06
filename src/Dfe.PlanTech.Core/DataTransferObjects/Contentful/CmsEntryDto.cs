using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsEntryDto
{
    public CmsEntrySystemDetailsDto Sys { get; set; } = null!;
    public string? Description { get; set; } = null!;

    protected CmsEntryDto BuildContentDto(ContentfulEntry contentComponent)
    {
        if (contentComponent is IDtoTransformableEntry entry)
        {
            return entry.AsDtoInternal();
        }

        throw new ArgumentException($"{nameof(ContentfulEntry)} cannot be transformed to a DTO.");
    }
}
