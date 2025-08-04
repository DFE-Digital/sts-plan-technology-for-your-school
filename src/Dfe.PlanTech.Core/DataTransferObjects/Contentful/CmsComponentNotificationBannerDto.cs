using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentNotificationBannerDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public CmsComponentTextBodyDto Text { get; set; } = null!;

    public CmsComponentNotificationBannerDto(ComponentNotificationBannerEntry notificationBannerEntry)
    {
        Id = notificationBannerEntry.Id;
        InternalName = notificationBannerEntry.InternalName;
        Text = notificationBannerEntry.Text.AsDto();
    }
}
