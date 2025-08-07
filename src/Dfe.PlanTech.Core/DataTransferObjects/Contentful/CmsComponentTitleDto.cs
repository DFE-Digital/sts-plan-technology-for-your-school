using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentTitleDto : CmsEntryDto
{
    public string? Id { get; set; }
    public string? InternalName { get; set; }
    public string Text { get; set; } = null!;

    public CmsComponentTitleDto() { }

    public CmsComponentTitleDto(ComponentTitleEntry titleEntry)
    {
        Id = titleEntry.Id;
        InternalName = titleEntry.InternalName;
        Text = titleEntry.Text;
    }
}
