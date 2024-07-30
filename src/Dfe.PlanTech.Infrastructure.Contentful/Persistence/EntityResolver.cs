using Contentful.Core.Configuration;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful;

/// <summary>
/// For mapping fields/properties of type <see chref="IContentComponent"/> to their concrete type,
/// when serialising the returned API response from Contentful
/// </summary>
public class EntityResolver(ILogger<EntityResolver> logger) : IContentTypeResolver
{
    public Dictionary<string, Type> Types => _types;

    private readonly ILogger<EntityResolver> _logger = logger;

    private readonly Dictionary<string, Type> _types = typeof(IContentComponent).Assembly.GetTypes()
                                                                        .Where(type => type.IsAssignableTo(typeof(IContentComponent)))
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