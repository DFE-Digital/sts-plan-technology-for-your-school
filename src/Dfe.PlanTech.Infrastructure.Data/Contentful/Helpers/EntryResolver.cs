using Contentful.Core.Configuration;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Helpers;

/// <summary>
/// For mapping fields/properties of type <see chref="ContentfulEntry"/> to their concrete type,
/// when serialising the returned API response from Contentful
/// </summary>
public class EntryResolver(ILogger<EntryResolver> logger) : IContentTypeResolver
{
    public Dictionary<string, Type> Types => _types;

    private readonly ILogger<EntryResolver> _logger = logger;

    private readonly Dictionary<string, Type> _types = ReflectionHelper.GetTypesInheritingFrom<ContentfulEntry>()
                                                                        .ToDictionary(type => type.Name.ToLower());

    /// <summary>
    /// Returns matching type for ID, or <see chref="MissingComponent"/> if none found.
    /// </summary>
    /// <param name="contentTypeId"></param>
    /// <returns></returns>
    public Type Resolve(string contentTypeId)
    {
        if (_types.TryGetValue(contentTypeId.ToLower(), out var type))
        {
            return type;
        }

        _logger.LogWarning("Could not find content type for ID {contentTypeId}", contentTypeId);
        return typeof(MissingComponent);
    }
}
