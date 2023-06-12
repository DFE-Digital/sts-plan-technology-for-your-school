using Dfe.PlanTech.Application.Caching.Interfaces;

namespace Dfe.PlanTech.Application.Core;

public class UrlHistory : IUrlHistory
{
    public const string CACHE_KEY = "URL_HISTORY";

    private readonly ICacher _cacher;

    public UrlHistory(ICacher cacher)
    {
        _cacher = cacher ?? throw new ArgumentNullException(nameof(cacher));
    }

    public Stack<string> History => _cacher.Get(CACHE_KEY, () => new Stack<string>())!;

    public string? LastVisitedUrl
    {
        get
        {
            var history = History;

            if (History != null && history.TryPeek(out string? lastVisitedPage))
            {
                return lastVisitedPage;
            }

            return null;
        }
    }

    public void AddUrlToHistory(string url)
    {
        var history = History;
        var lastVisitedUrl = LastVisitedUrl;

        bool isDuplicateUrl = !string.IsNullOrEmpty(lastVisitedUrl) && lastVisitedUrl.Equals(url);

        if (isDuplicateUrl)
        {
            return;
        }

        history.Push(url);
        SaveHistory(history);
    }

    public void RemoveLastUrl()
    {
        var history = History;
        history.TryPop(out string? _);

        SaveHistory(history);
    }

    private void SaveHistory(Stack<string> history)
    {
        _cacher.Set(CACHE_KEY, TimeSpan.FromHours(1), history);
    }

}
