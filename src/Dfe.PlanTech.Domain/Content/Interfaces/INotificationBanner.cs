namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Model for Notification Banner Component content type
/// </summary>
public interface INotificationBanner<out TTextBody>
where TTextBody : ITextBody
{
    /// <summary>
    /// Notification text
    /// </summary>
    public TTextBody Text { get; }
}
