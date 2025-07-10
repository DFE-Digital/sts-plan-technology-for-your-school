using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentJumpLinkDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string ComponentName { get; set; } = null!;
        public string JumpIdentifier { get; set; } = null!;

        public CmsComponentJumpLinkDto(ComponentJumpLinkEntry jumpLinkEntry)
        {
            Id = jumpLinkEntry.Id;
            ComponentName = jumpLinkEntry.ComponentName;
            JumpIdentifier = jumpLinkEntry.JumpIdentifier;
        }
    }
}
