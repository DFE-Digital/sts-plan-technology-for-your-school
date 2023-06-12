namespace Dfe.PlanTech.Application.Core;

/// <summary>
/// For tracking user's URL history
/// </summary>
public interface IUrlHistory
{
    /// <summary>
    /// Users history
    /// </summary>
    public Stack<string> History { get; }
    
    /// <summary>
    /// The last URL a user visited (or null if none)
    /// </summary>
    public string? LastVisitedUrl { get; }
    
    /// <summary>
    /// Adds the URL to user history and save
    /// </summary>
    /// <param name="url"></param>
    public void AddUrlToHistory(string url);
    
    /// <summary>
    /// Remove last visited URL from history
    /// </summary>
    public void RemoveLastUrl();
}