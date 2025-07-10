using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentInsetTextDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string Text { get; init; } = null!;

        public CmsComponentInsetTextDto(ComponentInsetTextEntry insetTextEntry)
        {
            Id = insetTextEntry.Id;
            InternalName = insetTextEntry.InternalName;
            Text = insetTextEntry.Text;
        }
    }
}
