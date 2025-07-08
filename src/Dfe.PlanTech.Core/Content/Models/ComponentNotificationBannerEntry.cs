using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentNotificationBannerEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public TextBody Text { get; init; } = null!;
}
