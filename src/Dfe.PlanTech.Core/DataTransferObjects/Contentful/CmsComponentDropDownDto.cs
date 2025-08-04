using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentDropDownDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public CmsRichTextContentDto? Content { get; set; } = null!;

    public CmsComponentDropDownDto(ComponentDropDownEntry dropDownEntry)
    {
        Id = dropDownEntry.Id;
        InternalName = dropDownEntry.InternalName;
        Title = dropDownEntry.Title;
        Content = dropDownEntry.Content?.AsDto();
    }
}
