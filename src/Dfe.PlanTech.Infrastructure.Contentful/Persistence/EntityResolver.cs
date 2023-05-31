using Contentful.Core.Configuration;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful;

public class EntityResolver : IContentTypeResolver
{
    public Dictionary<string, Type> _types = typeof(IContentComponent).Assembly.GetTypes()
                                                                        .Where(type => type.IsAssignableTo(typeof(IContentComponent)))
                                                                        .ToDictionary(type => type.Name.ToLower());

    public Type Resolve(string contentTypeId) => _types.TryGetValue(contentTypeId.ToLower(), out var type) ? type : throw new Exception($"Could not find type for {contentTypeId}");
}