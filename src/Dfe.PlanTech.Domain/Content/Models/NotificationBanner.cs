using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class NotificationBanner : ContentComponent, INotificationBanner<TextBody>
{
    public TextBody Text { get; init; } = null!;
}
