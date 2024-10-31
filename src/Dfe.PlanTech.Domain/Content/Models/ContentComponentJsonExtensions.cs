using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dfe.PlanTech.Domain.Helpers;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Provides Json type info mapping for <see cref="ContentComponent"/> to handle deserialising objects to their concrete classes 
/// </summary>
public static class ContentComponentJsonExtensions
{
    private const string TypeDiscriminatorName = "$contentcomponenttype";

    /// <summary>
    /// Gets all classes that inherit from <see cref="ContentComponent"/> and creates <see cref="JsonDerivedType"/> mapping information for deserialisation 
    /// </summary>
    private static readonly List<JsonDerivedType> ContentComponentTypes = ReflectionHelpers
        .GetTypesInheritingFrom<ContentComponent>()
        .Select(type => new JsonDerivedType(type, type.Name))
        .ToList();

    private static readonly Type ContentComponentType = typeof(ContentComponent);

    private static JsonPolymorphismOptions? _contentComponentPolymorphismOptions = null;

    private static JsonPolymorphismOptions ContentComponentPolymorphismOptions =>
        _contentComponentPolymorphismOptions ??= CreateJsonPolymorphismOptions();

    /// <summary>
    /// Creates polymorphism support for the <see cref="ContentComponent"/> class.
    /// </summary>
    /// <returns></returns>
    private static JsonPolymorphismOptions CreateJsonPolymorphismOptions()
    {
        var options = new JsonPolymorphismOptions()
        {
            TypeDiscriminatorPropertyName = TypeDiscriminatorName,
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        foreach (var derivedType in ContentComponentTypes)
        {
            options.DerivedTypes.Add(derivedType);
        }

        return options;
    }

    /// <summary>
    /// Adds polymorphism support for the <see cref="ContentComponent"/> class.
    /// </summary>
    /// <param name="jsonTypeInfo"></param>
    public static void AddContentComponentPolymorphicInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Type != ContentComponentType)
            return;

        jsonTypeInfo.PolymorphismOptions = ContentComponentPolymorphismOptions;
    }
}
