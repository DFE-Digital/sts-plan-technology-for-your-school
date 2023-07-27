using System.Security.Claims;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Caching.Models;

public class UrlHistory : BaseCacher, IUrlHistory
{
    public const string CACHE_KEY = "URL_HISTORY";

    public UrlHistory(ICacher cacher, IHttpContextAccessor httpContextAccessor) : base(CACHE_KEY, cacher, httpContextAccessor)
    {
    }

    public Task<Stack<Uri>> History => _cacher.GetAsync(KeyForUser, () => new Stack<Uri>())!;

    public async Task<Uri?> GetLastVisitedUrl()
    {
        if (!UserIsAuthenticated) return null;

        var history = await History;

        if (History != null && history.TryPeek(out Uri? lastVisitedPage))
        {
            return lastVisitedPage;
        }

        return null;
    }

    public async Task AddUrlToHistory(Uri url)
    {
        if (!UserIsAuthenticated) return;

        var history = await History;
        history.Push(url);
        await SaveHistory(history);
    }

    public async Task RemoveLastUrl()
    {
        if (!UserIsAuthenticated) return;

        var history = await History;
        history.TryPop(out Uri? _);

        await SaveHistory(history);
    }


    private Task SaveHistory(Stack<Uri> history) => _cacher.SetAsync(KeyForUser, history);
}
