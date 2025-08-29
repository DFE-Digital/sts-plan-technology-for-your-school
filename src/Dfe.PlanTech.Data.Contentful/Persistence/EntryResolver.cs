using Contentful.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Data.Contentful.Persistence;

/// <summary>
/// For mapping fields/properties of type <see chref="ContentfulEntry"/> to their concrete type,
/// when serialising the returned API response from Contentful
/// </summary>
public class EntryResolver(
) : IContentTypeResolver
{
    public Dictionary<string, Type> Types => _types;

    private readonly Dictionary<string, Type> _types = ReflectionHelper.GetTypesInheritingFrom<ContentfulEntry>()
                                                                       .ToDictionary(type => type.Name.ToLower());

    /// <summary>
    /// Returns matching type for ID, or <see chref="MissingComponent"/> if none found.
    /// </summary>
    /// <param name="contentTypeId"></param>
    /// <returns></returns>
    public Type Resolve(string contentTypeId)
    {
        if (ContentfulContentTypeConstants.ContentTypeToEntryClassTypeMap.TryGetValue(contentTypeId.ToLower(), out var type))
        {
            return type;
        }

        return typeof(MissingComponentEntry);
    }
}
