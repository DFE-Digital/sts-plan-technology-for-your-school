using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsEntryDto
    {
        public CmsEntrySystemDetailsDto Sys { get; set; } = null!;
        public string? Description { get; set; } = null!;

        protected CmsEntryDto BuildContentDto(ContentComponent contentComponent)
        {
            if (contentComponent is IDtoTransformable entry)
            {
                return entry.AsDtoInternal();
            }

            throw new ArgumentException($"{nameof(ContentComponent)} cannot be transformed to a DTO.");
        }
    }
}
