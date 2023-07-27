namespace Dfe.PlanTech.Application.Caching.Interfaces;

/// <summary>
/// For tracking user's URL history
/// </summary>
public interface IUrlHistory
{
    public const string DEFAULT_PAGE = "self-assessment";

    /// <summary>
    /// Users history
    /// </summary>
    public Task<Stack<Uri>> History { get; }

    /// <summary>
    /// The last URL a user visited (or null if none)
    /// </summary>
    public Task<Uri?> GetLastVisitedUrl();

    /// <summary>
    /// Adds the URL to user history and save
    /// </summary>
    /// <param name="url"></param>
    public Task AddUrlToHistory(Uri url);

    /// <summary>
    /// Remove last visited URL from history
    /// </summary>
    public Task RemoveLastUrl();

    public bool UserIsAuthenticated { get; }
}