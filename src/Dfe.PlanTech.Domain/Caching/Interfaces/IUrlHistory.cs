namespace Dfe.PlanTech.Domain.Caching.Interfaces;

/// <summary>
/// For tracking user's URL history
/// </summary>
public interface IUrlHistory
{
    /// <summary>
    /// Users history
    /// </summary>
    public Stack<Uri> History { get; }

    /// <summary>
    /// The last URL a user visited (or null if none)
    /// </summary>
    public Uri? LastVisitedUrl { get; }

    /// <summary>
    /// Adds the URL to user history and save
    /// </summary>
    /// <param name="url"></param>
    public void AddUrlToHistory(Uri url);

    /// <summary>
    /// Remove last visited URL from history
    /// </summary>
    public void RemoveLastUrl();
}
