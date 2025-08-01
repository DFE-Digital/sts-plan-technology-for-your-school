using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Extensions;
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
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            .WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<IContentfulEntry>)
            .WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<Entry<ContentComponent>>),
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
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
