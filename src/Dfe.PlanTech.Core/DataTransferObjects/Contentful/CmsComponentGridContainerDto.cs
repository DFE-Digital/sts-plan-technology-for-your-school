using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentGridContainerDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string? InternalName { get; set; }
        public List<CmsComponentCardDto>? Content { get; set; }

        public CmsComponentGridContainerDto(ComponentGridContainerEntry gridContainerEntry)
        {
            Id = gridContainerEntry.Id;
            InternalName = gridContainerEntry.InternalName;
            Content = gridContainerEntry.Content?.Select(BuildContentDto).ToList();
        }
    }
}
