using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Contentful.Core.Models;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.Extensions;

/// <summary>
/// Provides JSON type info mapping for <see cref="ContentfulEntry"/> to handle deserialising objects to their concrete classes
/// </summary>
public static class ContentComponentJsonExtensions
{
    /// <summary>
    /// Adds polymorphism support for the <see cref="ContentfulEntry"/> class.
    /// </summary>
    /// <param name="jsonTypeInfo"></param>
    public static void AddContentComponentPolymorphicInfo<TType>(JsonTypeInfo jsonTypeInfo)
    {
        if (!jsonTypeInfo.Type.Equals(typeof(TType)))
            return;

        if (jsonTypeInfo.PolymorphismOptions is not null)
            return;

        var options = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = $"${typeof(TType).Name.ToLower()}",
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        foreach (var derivedType in GetInheritingTypes(typeof(TType)))
        {
            options.DerivedTypes.Add(derivedType);
        }

        jsonTypeInfo.PolymorphismOptions = options;
    }

    /// <summary>
    /// Validates that <see cref="ContentfulContentTypeConstants"/> is in sync with all concrete
    /// <see cref="ContentfulEntry"/> subtypes discovered via reflection. Throws on mismatch so
    /// the application fails fast at startup if a new entry type is added without registering it.
    /// </summary>
    public static void ValidateContentfulTypeMapping()
    {
        var contentfulEntryTypes = ReflectionHelper.GetTypesInheritingFrom<ContentfulEntry>();
        var entryContentfulEntryTypes = ReflectionHelper.GetTypesInheritingFrom<
            Entry<ContentfulEntry>
        >();
        var contentfulFieldTypes = ReflectionHelper.GetTypesInheritingFrom<ContentfulField>();

        var reflectedTypes = contentfulEntryTypes
            .Union(entryContentfulEntryTypes)
            .Union(contentfulFieldTypes)
            .Where(t => t.IsConcreteClass() && t.HasParameterlessConstructor())
            .ToHashSet();

        var mappedTypes = ContentfulContentTypeConstants.EntryTypeToContentTypeMap.Keys.ToHashSet();

        var inReflectionNotInMap = reflectedTypes.Except(mappedTypes).ToList();
        var inMapNotInReflection = mappedTypes.Except(reflectedTypes).ToList();

        if (inReflectionNotInMap.Count == 0 && inMapNotInReflection.Count == 0)
            return;

        var sb = new StringBuilder(
            "ContentfulContentTypeConstants is out of sync with concrete ContentfulEntry subtypes."
        );
        if (inReflectionNotInMap.Count > 0)
            sb.Append(
                $"\nIn code but missing from ContentfulContentTypeConstants: {string.Join(", ", inReflectionNotInMap.Select(t => t.Name))}"
            );
        if (inMapNotInReflection.Count > 0)
            sb.Append(
                $"\nIn ContentfulContentTypeConstants but not found as concrete subtypes: {string.Join(", ", inMapNotInReflection.Select(t => t.Name))}"
            );

        throw new InvalidOperationException(sb.ToString());
    }

    /// <summary>
    /// Gets all types from <see cref="ContentfulContentTypeConstants"/> that are assignable to
    /// <paramref name="type"/> and are valid for JSON polymorphic serialisation.
    /// </summary>
    private static List<JsonDerivedType> GetInheritingTypes(Type type) =>
        [
            .. ContentfulContentTypeConstants
                .EntryTypeToContentTypeMap.Where(kvp =>
                    type.IsAssignableFrom(kvp.Key)
                    && kvp.Key.IsConcreteClass()
                    && kvp.Key.HasParameterlessConstructor()
                )
                .Select(kvp => new JsonDerivedType(kvp.Key, kvp.Value)),
        ];
}
