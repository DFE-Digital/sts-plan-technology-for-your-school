using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.SignIn.Enums;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Caching.Models;

public class UrlHistory : IUrlHistory
{
    public const string CACHE_KEY = "URL_HISTORY";
    public const string CLAIM_TYPE = ClaimConstants.VerifiedEmail;

    private readonly ICacher _cacher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UrlHistory(ICacher cacher, IHttpContextAccessor httpContextAccessor)
    {
        _cacher = cacher ?? throw new ArgumentNullException(nameof(cacher));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public Task<Stack<Uri>> History => _cacher.GetAsync(GetKeyForUser(), () => new Stack<Uri>())!;

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

    private Task SaveHistory(Stack<Uri> history) => _cacher.SetAsync(GetKeyForUser(), history);

    private string GetKeyForUser()
    {
        var userClaims = (_httpContextAccessor.HttpContext?.User?.Claims) ??
                        throw new NullReferenceException("User has no claims");

        var currentUser = userClaims!.FirstOrDefault(claim => claim.Type == CLAIM_TYPE) ??
                        throw new KeyNotFoundException($"Could not find user claim for {CLAIM_TYPE}");

        return $"{CACHE_KEY}:{currentUser.Value}";
    }
}
