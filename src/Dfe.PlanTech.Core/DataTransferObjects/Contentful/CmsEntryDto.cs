using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsEntryDto
    {
        public CmsEntrySystemDetailsDto Sys { get; set; } = null!;
        public string? Description { get; set; } = null!;

        protected CmsEntryDto BuildContentDto(Entry<ContentComponent> contentComponent)
        {
            if (contentComponent is IDtoTransformable entry)
            {
                return entry.AsDtoInternal();
            }

            throw new ArgumentException($"{nameof(IContentfulEntry)} cannot be transformed to a DTO.");
        }
    }
}
