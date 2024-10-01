using Dfe.PlanTech.Domain.Caching.Interfaces;

namespace Dfe.PlanTech.Application.Caching.Models;

public class UrlHistory : IUrlHistory
{
    public const string CACHE_KEY = "URL_HISTORY";

    private readonly ICacher _cacher;

    public UrlHistory(ICacher cacher)
    {
        _cacher = cacher ?? throw new ArgumentNullException(nameof(cacher));
    }

    public Stack<Uri> History => _cacher.Get(CACHE_KEY, () => new Stack<Uri>())!;

    public Uri? LastVisitedUrl
    {
        get
        {
            var history = History;

            if (History != null && history.TryPeek(out Uri? lastVisitedPage))
            {
                return lastVisitedPage;
            }

            return null;
        }
    }

    public void AddUrlToHistory(Uri url)
    {
        var history = History;
        history.Push(url);
        SaveHistory(history);
    }

    public void RemoveLastUrl()
    {
        var history = History;
        history.TryPop(out Uri? _);

        SaveHistory(history);
    }

    private void SaveHistory(Stack<Uri> history)
    {
        _cacher.Set(CACHE_KEY, TimeSpan.FromHours(1), history);
    }
}
