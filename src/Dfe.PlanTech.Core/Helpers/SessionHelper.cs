using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Core.Helpers;

public static class SessionHelper
{
    public static void ClearFocusedEstablishment(this ISession session)
    {
        session.Remove(SessionConstants.FocusedEstablishmentKey);
    }

    public static SelectedEstablishmentDataModel? GetFocusedEstablishment(this ISession session)
    {
        return
            session.GetValue(SessionConstants.FocusedEstablishmentKey)
                is SelectedEstablishmentDataModel focusedEstablishment
            ? focusedEstablishment
            : null;
    }

    public static void SetFocusedEstablishment(
        this ISession session,
        int id,
        string urn,
        string name
    )
    {
        var establishment = new SelectedEstablishmentDataModel
        {
            Id = id,
            Urn = urn,
            Name = name,
        };
        session.SetValue(SessionConstants.FocusedEstablishmentKey, establishment);
    }

    public static void ClearFocusedAndSelectedEstablishments(this ISession session)
    {
        session.Remove(SessionConstants.FocusedEstablishmentKey);
        session.Remove(SessionConstants.SelectedEstablishmentsKey);
    }

    public static IReadOnlyList<int> GetSelectedEstablishmentIds(this ISession session)
    {
        var establishments = session.GetValue(SessionConstants.SelectedEstablishmentsKey);
        if (
            establishments is not IEnumerable<SelectedEstablishmentDataModel> selectedEstablishments
        )
        {
            return [];
        }

        return selectedEstablishments.Select(e => e.Id).ToArray();
    }

    public static IReadOnlyList<string> GetSelectedEstablishmentNames(this ISession session)
    {
        var establishments = session.GetValue(SessionConstants.SelectedEstablishmentsKey);

        if (
            establishments is not IEnumerable<SelectedEstablishmentDataModel> selectedEstablishments
        )
        {
            return [];
        }

        return selectedEstablishments.Select(e => e.Name).ToArray();
    }

    public static IReadOnlyList<SelectedEstablishmentDataModel> GetSelectedEstablishments(
        this ISession session
    )
    {
        return
            session.GetValue(SessionConstants.SelectedEstablishmentsKey)
                is IEnumerable<SelectedEstablishmentDataModel> selectedEstablishments
            ? selectedEstablishments.ToArray()
            : [];
    }

    public static void SetSelectedEstablishments(
        this ISession session,
        int id,
        string urn,
        string name
    )
    {
        var establishment = new SelectedEstablishmentDataModel
        {
            Id = id,
            Urn = urn,
            Name = name,
        };
        session.SetSelectedEstablishments([establishment]);
    }

    public static void SetSelectedEstablishments(
        this ISession session,
        IList<SelectedEstablishmentDataModel> selectedEstablishments
    )
    {
        foreach (var establishment in selectedEstablishments)
        {
            if (establishment.Id == 0)
            {
                throw new InvalidDataException("Establishment ID cannot be 0");
            }

            if (string.IsNullOrWhiteSpace(establishment.Name))
            {
                throw new InvalidDataException("Name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(establishment.Urn))
            {
                throw new InvalidDataException("URN cannot be empty");
            }
        }

        session.SetValue(SessionConstants.SelectedEstablishmentsKey, selectedEstablishments);
    }

    private static Object? GetValue(this ISession session, string key)
    {
        if (!SessionConstants.SessionKeyTypes.TryGetValue(key, out var type))
        {
            throw new InvalidOperationException("Session does not expect that key");
        }

        var value = session.GetString(key);
        return value != null ? JsonSerializer.Deserialize(value, type) : null;
    }

    private static void SetValue<T>(this ISession session, string key, T value)
    {
        if (
            !SessionConstants.SessionKeyTypes.TryGetValue(key, out var type)
            || !type.Equals(typeof(T))
        )
        {
            throw new InvalidOperationException(
                "Session does not expect that key or value is of wrong type for key"
            );
        }

        session.SetString(key, JsonSerializer.Serialize(value));
    }
}
