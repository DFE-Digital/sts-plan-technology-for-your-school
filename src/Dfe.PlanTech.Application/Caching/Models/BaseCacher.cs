using System.Security.Claims;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Caching.Models;

public class BaseCacher
{
    public readonly string Key;
    public const string USER_CLAIM = ClaimTypes.Email;

    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ICacher _cacher;

    protected string KeyForUser
    {
        get
        {
            var userClaims = (_httpContextAccessor.HttpContext?.User?.Claims) ??
                            throw new NullReferenceException("User has no claims");

            var currentUser = userClaims!.FirstOrDefault(claim => claim.Type == USER_CLAIM) ??
                            throw new KeyNotFoundException($"Could not find user claim for {USER_CLAIM}");

            return $"{Key}:{currentUser.Value}";
        }
    }

    public bool UserIsAuthenticated => _httpContextAccessor.HttpContext.User?.Identity?.IsAuthenticated == true;

    public BaseCacher(string key, ICacher cacher, IHttpContextAccessor httpContextAccessor)
    {
        Key = key;
        _cacher = cacher;
        _httpContextAccessor = httpContextAccessor;
    }
}