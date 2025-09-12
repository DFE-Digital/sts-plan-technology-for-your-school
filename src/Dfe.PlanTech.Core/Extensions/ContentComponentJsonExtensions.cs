using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
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
    /// Gets all types inheriting <see cref="Type"/> that would be valid for serialisation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static List<JsonDerivedType> GetInheritingTypes(Type type) => [.. ReflectionHelper
        .GetTypesInheritingFrom(type)
        .Where(derivedType => derivedType != type &&
                              derivedType.IsConcreteClass() &&
                              derivedType.HasParameterlessConstructor())
        .Select(type => new JsonDerivedType(type, ContentfulContentTypeConstants.EntryClassToContentTypeMap[type.Name]))];
}
