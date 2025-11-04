namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.Configuration;

/// <summary>
/// Configuration for the maintenance page messaging.
/// Supports custom paragraphs via environment variables for flexible messaging during maintenance windows.
/// </summary>
public class MaintenanceConfiguration
{
    /// <summary>
    /// Array of message paragraphs to display on the maintenance page.
    /// Each string becomes a separate paragraph.
    /// If empty or null, a default message is displayed.
    ///
    /// Example configuration via environment variables:
    ///   Maintenance__MessageParagraphs__0 = "The service will be unavailable from 5pm on Monday 4th November."
    ///   Maintenance__MessageParagraphs__1 = "You will be able to use the service from 9am on Tuesday 5th November."
    /// </summary>
    public List<string> MessageParagraphs { get; set; } = new();
}
