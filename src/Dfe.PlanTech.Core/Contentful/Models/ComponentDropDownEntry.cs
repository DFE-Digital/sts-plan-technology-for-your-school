using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentDropDownEntry: TransformableEntry<ComponentDropDownEntry, CmsComponentDropDownDto>
{
    public string InternalName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public RichTextContentEntry? Content { get; set; } = null!;

    protected override Func<ComponentDropDownEntry, CmsComponentDropDownDto> Constructor => entry => new(entry);
}
