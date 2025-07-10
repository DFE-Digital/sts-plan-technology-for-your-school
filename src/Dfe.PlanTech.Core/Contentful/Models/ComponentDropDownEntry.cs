using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentDropDownEntry: TransformableEntry<ComponentDropDownEntry, CmsComponentDropDownDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public RichTextContent? Content { get; set; } = null!;

    public ComponentDropDownEntry() : base(entry => new CmsComponentDropDownDto(entry)) { }
}
