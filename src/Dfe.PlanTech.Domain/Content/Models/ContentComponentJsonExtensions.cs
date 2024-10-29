using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dfe.PlanTech.Domain.Helpers;

namespace Dfe.PlanTech.Domain.Content.Models;

public static class ContentComponentJsonExtensions
{
    private const string TypeDiscriminatorName = "$contentcomponenttype";
    private static readonly List<JsonDerivedType> ContentComponentTypes = ReflectionHelpers
        .GetTypesInheritingFrom<ContentComponent>()
        .Select(type => new JsonDerivedType(type, type.Name))
        .ToList();

    private static readonly Type ContentComponentType = typeof(ContentComponent);

    private static JsonPolymorphismOptions? _contentComponentPolymorphismOptions = null;

    private static JsonPolymorphismOptions ContentComponentPolymorphismOptions =>
        _contentComponentPolymorphismOptions ??= CreateJsonPolymorphismOptions();

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

    public static void AddContentComponentPolymorphicInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Type != ContentComponentType)
            return;

        jsonTypeInfo.PolymorphismOptions = ContentComponentPolymorphismOptions;
    }
}
