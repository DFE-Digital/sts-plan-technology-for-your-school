using System.Reflection;
using Dfe.PlanTech.Core.Attributes;
using Dfe.PlanTech.Core.Constants;

namespace Dfe.PlanTech.Core.Helpers;

public static class ContentfulContentTypeHelper
{
    public static string GetContentTypeName<TEntry>()
    {
        var type = typeof(TEntry);
        var typeName = type.Name;
        if (
            ContentfulContentTypeConstants.EntryClassToContentTypeMap.TryGetValue(
                typeName,
                out var contentfulContentTypeId
            )
        )
        {
            return contentfulContentTypeId!;
        }

        // This is used for testing
        var attribute = type.GetCustomAttribute<ContentfulTypeAttribute>();
        if (attribute is not null)
        {
            return attribute.Id;
        }

        throw new InvalidOperationException(
            $"Could not find content type ID for class type {typeName}"
        );
    }
}
