using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Dfe.PlanTech.Domain.Content.Models;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public static class JsonSerialiser
{
    private static readonly JsonSerializerOptions JsonSerialiserOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo),
    };

    public static string Serialise<T>(this T obj) => JsonSerializer.Serialize(obj, JsonSerialiserOptions);

    public static T? Deserialise<T>(this RedisValue redisValue) => JsonSerializer.Deserialize<T>(redisValue!, JsonSerialiserOptions);
}
