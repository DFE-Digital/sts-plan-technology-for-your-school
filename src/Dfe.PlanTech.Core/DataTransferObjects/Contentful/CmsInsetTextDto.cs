using Dfe.PlanTech.Core.Content.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsInsetTextDto
    {
        public CmsInsetTextDto(CmsComponentInsetTextDto insetTextEntry)
        {
            Id = insetTextEntry.Id;
            InternalName = insetTextEntry.InternalName;
            Text = insetTextEntry.Text;
        }

        public string Id { get; init; } = null!;
        public string InternalName { get; set; } = null!;
        public string Text { get; init; } = null!;
    }
}
