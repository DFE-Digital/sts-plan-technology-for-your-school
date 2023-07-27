using Dfe.PlanTech.Application.Caching.Interfaces;

namespace Dfe.PlanTech.Application.Caching.Models;

public class UrlHistory : IUrlHistory
{
    public const string CACHE_KEY = "URL_HISTORY";

    private readonly ICacher _cacher;

    public UrlHistory(ICacher cacher)
    {
        _cacher = cacher ?? throw new ArgumentNullException(nameof(cacher));
    }

    public Task<Stack<Uri>> History => _cacher.GetAsync(CACHE_KEY, () => new Stack<Uri>())!;

    public async Task<Uri?> GetLastVisitedUrl()
    {
        var history = await History;

        if (History != null && history.TryPeek(out Uri? lastVisitedPage))
        {
            return lastVisitedPage;
        }

        return null;
    }

    public async Task AddUrlToHistory(Uri url)
    {
        var history = await History;
        history.Push(url);
        await SaveHistory(history);
    }

    public async Task RemoveLastUrl()
    {
        var history = await History;
        history.TryPop(out Uri? _);

        await SaveHistory(history);
    }

    private Task SaveHistory(Stack<Uri> history) => _cacher.SetAsync(CACHE_KEY, history);
}
