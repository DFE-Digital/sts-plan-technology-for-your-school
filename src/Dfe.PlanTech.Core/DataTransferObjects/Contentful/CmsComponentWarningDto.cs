using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentWarningDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public CmsComponentTextBodyDto Text { get; init; } = null!;

        public CmsComponentWarningDto(ComponentWarningEntry warningEntry)
        {
            Id = warningEntry.Id;
            InternalName = warningEntry.InternalName;
            Text = warningEntry.Text.AsDto();
        }
    }
}
