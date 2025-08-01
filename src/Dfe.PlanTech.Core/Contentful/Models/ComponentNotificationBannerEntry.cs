using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentNotificationBannerEntry: TransformableEntry<ComponentNotificationBannerEntry, CmsComponentNotificationBannerDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentTextBodyEntry Text { get; init; } = null!;

    public ComponentNotificationBannerEntry() : base(entry => new CmsComponentNotificationBannerDto(entry)) {}
}
