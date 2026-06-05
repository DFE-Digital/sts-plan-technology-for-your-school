using Dfe.PlanTech.Core.Constants;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Dfe.PlanTech.Core.Helpers;

public static class SessionHelper
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        if (!SessionConstants.SessionKeyTypes.TryGetValue(key, out var type) || !type.Equals(typeof(T)))
        {
            throw new InvalidOperationException("Session does not expect that key or value is of wrong type for key");
        }

        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static Object? Get(this ISession session, string key)
    {
        if(!SessionConstants.SessionKeyTypes.TryGetValue(key, out var type))
        {
            throw new InvalidOperationException("Session does not expect that key");
        }

        var value = session.GetString(key);
        return value != null ? JsonSerializer.Deserialize(value, type) : null;
    }
}
