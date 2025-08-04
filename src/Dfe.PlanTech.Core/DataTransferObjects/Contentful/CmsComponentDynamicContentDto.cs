using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentDynamicContentDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; init; } = null!;
    public string DynamicField { get; init; } = null!;

    public CmsComponentDynamicContentDto(ComponentDynamicContentEntry dynamicContentEntry)
    {
        Id = dynamicContentEntry.Id;
        InternalName = dynamicContentEntry.InternalName;
        DynamicField = dynamicContentEntry.DynamicField;
    }
}
