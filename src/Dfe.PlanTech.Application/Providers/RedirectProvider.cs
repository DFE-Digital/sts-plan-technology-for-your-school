using System.Reflection;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Providers;

public class RedirectProvider : IRedirectProvider
{
    private readonly ILogger<RedirectProvider> _logger;
    private readonly IContentfulService _contentfulService;

    private readonly Lazy<HashSet<string>> _knownPaths;
    private readonly Lazy<Task<Dictionary<string, string>>> _redirectsTask;

    public RedirectProvider(ILogger<RedirectProvider> logger, IContentfulService contentfulService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contentfulService =
            contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));

        _knownPaths = new(BuildKnownPaths, LazyThreadSafetyMode.ExecutionAndPublication);
        _redirectsTask = new(BuildRedirectsAsync, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public bool IsStaticPath(string path)
    {
        path = path.ToLowerInvariant();

        return path.StartsWith("api")
            || _knownPaths.Value.Any(kp => path.Equals(kp, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string?> TryGetRedirect(string path)
    {
        var redirects = await _redirectsTask.Value;

        return redirects.TryGetValue(path, out string? redirectTo) ? redirectTo : null;
    }

    private static HashSet<string> BuildKnownPaths()
    {
        List<string> devPaths =
        [
            "/prod",
            "/healthy",
            "/auth/sign-out",
            "/ConfirmCheckAnswers",
            "/service-unavailable",
            "/apple-touch-icon.png",
            "/apple-touch-icon-precomposed.png",
        ];

        var urlConstantPaths = typeof(UrlConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Where(val => val.StartsWith('/'))
            .ToList();

        return
        [
            .. devPaths
                .Union(urlConstantPaths)
                .Select(val => val.TrimStart('/').Trim().ToLowerInvariant())
                .Distinct(),
        ];
    }

    private async Task<Dictionary<string, string>> BuildRedirectsAsync()
    {
        try
        {
            var redirectEntries = await _contentfulService.GetRedirectsAsync();
            var redirectGroups = redirectEntries
                .SelectMany(r =>
                    r.RedirectFromList.Select(rl => new KeyValuePair<string, string>(
                        rl,
                        r.RedirectTo
                    ))
                )
                .GroupBy(kvp => kvp.Key);

            var duplicateRedirectPaths = redirectGroups.Where(rg => rg.Count() > 1).ToList();
            if (duplicateRedirectPaths.Count != 0)
            {
                _logger.LogWarning(
                    "Duplicate redirects detected in Contentful: {DuplicatePaths}",
                    string.Join(", ", duplicateRedirectPaths.Select(drp => drp.Key))
                );
            }

            var redirects = redirectGroups
                .Where(rg => rg.Count() == 1)
                .ToDictionary(
                    rg => rg.Key,
                    rg => rg.First().Value,
                    StringComparer.OrdinalIgnoreCase
                );

            // Return flattened links (A-B-C -> A-C) and remove circular references
            return FlattenRedirects(redirects);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to build redirect dictionary from Contentful. Will not serve redirects."
            );
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private enum VisitState
    {
        Seen,
        Resolved,
        Circular,
    }

    /// <summary>
    /// Resolves each redirect to its eventual target (A-B-C becomes A-C, B-C)
    /// and excludes paths which loop back on themselves in some way.
    /// </summary>
    private Dictionary<string, string> FlattenRedirects(Dictionary<string, string> redirects)
    {
        var state = new Dictionary<string, VisitState>(StringComparer.OrdinalIgnoreCase);
        var circularPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var targetPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var fromPath in redirects.Keys)
        {
            if (state.ContainsKey(fromPath))
            {
                continue;
            }

            var (chain, targetPath) = WalkPath(
                redirects,
                state,
                circularPaths,
                targetPaths,
                fromPath
            );
            if (targetPath != null)
            {
                // Chain ends in a valid target.
                foreach (var path in chain)
                {
                    state[path] = VisitState.Resolved;
                    targetPaths[path] = targetPath;
                }
            }
        }

        if (circularPaths.Count != 0)
        {
            _logger.LogWarning(
                "Circular redirects detected in Contentful. Excluding paths from the redirect map: {CircularPaths}",
                string.Join(", ", circularPaths)
            );
        }

        return targetPaths;
    }

    private static (List<string> chain, string? targetPath) WalkPath(
        Dictionary<string, string> redirects,
        Dictionary<string, VisitState> state,
        HashSet<string> circularPaths,
        Dictionary<string, string> targetPaths,
        string fromPath
    )
    {
        var chain = new List<string>();

        while (redirects.TryGetValue(fromPath, out var toPath))
        {
            if (state.TryGetValue(fromPath, out var currentState))
            {
                if (currentState == VisitState.Resolved)
                {
                    return (chain, targetPaths[fromPath]);
                }

                // If the state is Seen or Circular then everything
                // before this feeds into a cycle, and everything from
                // this point on is a cycle.
                foreach (var path in chain)
                {
                    state[path] = VisitState.Circular;
                    circularPaths.Add(path);
                }

                return (chain, null);
            }

            state[fromPath] = VisitState.Seen;
            chain.Add(fromPath);
            fromPath = toPath;
        }

        return (chain, fromPath);
    }
}
