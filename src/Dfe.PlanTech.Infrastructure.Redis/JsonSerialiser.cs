using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

/// <summary>
/// Provides functionality for serialising the <see cref="ContentComponent"/> class, due to issues with a base class
/// </summary>
public static class JsonSerialiser
{
    /// <summary>
    /// Default JSON options to use; adds base class type info resolver from <see cref="ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo"/>
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerialiserOptions = new()
    {
        TypeInfoResolver =
            new DefaultJsonTypeInfoResolver().WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<IContentComponent>)
            .WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<ContentComponent>)
            .WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<ContentBase>),
        ReferenceHandler = ReferenceHandler.Preserve
    };

    /// <summary>
    /// Serialise the object using the <see cref="JsonSerialiserOptions"/> options
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string Serialise<T>(this T obj) => JsonSerializer.Serialize(obj, JsonSerialiserOptions);

    /// <summary>
    /// Deserialise the object using the <see cref="JsonSerialiserOptions"/> options
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T? Deserialise<T>(this RedisValue redisValue) => JsonSerializer.Deserialize<T>(redisValue!, JsonSerialiserOptions);
}
