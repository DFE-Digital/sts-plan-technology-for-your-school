using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Core.Helpers;

public static class SessionHelper
{
    public static object? GetValue(this ISession session, string key)
    {
        if (!SessionConstants.SessionKeyTypes.TryGetValue(key, out var type))
        {
            throw new InvalidOperationException("Session does not expect that key");
        }

        var value = session.GetString(key);
        return value != null ? JsonSerializer.Deserialize(value, type) : null;
    }

    public static void SetValue<T>(this ISession session, string key, T value)
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

    /* **** USER ESTABLISHMENT ****
     * This is the database record in dbo.establishment which represents the user's organisation.
     * For a school user, it will be a school.
     * For a SAT, SSAT, or MAT user, it will be the trust.
     */

    public static SqlEstablishmentDto? GetUserEstablishment(this ISession session)
    {
        return
            session.GetValue(SessionConstants.UserEstablishmentKey)
                is SqlEstablishmentDto establishment
            ? establishment
            : null;
    }

    public static void SetUserEstablishment(
        this ISession session,
        SqlEstablishmentDto establishment
    )
    {
        session.SetValue(SessionConstants.UserEstablishmentKey, establishment);
    }

    /* **** FOCUSED ESTABLISHMENT ****
     * This is the dbo.establishment ID, URN, and name of the school the user is viewing.
     * For a school user, it will represent their school.
     * For a SAT or SSAT user, it will represent the single school within that trust.
     * For a MAT user, it will represent the single school they have chosen to view.
     */

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
            EstablishmentId = id,
            Urn = urn,
            Name = name,
        };
        session.SetValue(SessionConstants.FocusedEstablishmentKey, establishment);
    }

    /* **** GROUP SCHOOLS ****
     * These are the dbo.establishment IDs, URNs, and names of the schools within a group user's group.
     * School, SAT, and SSAT users should never have access to this.
     * For a MAT user, it will represent the schools within their group according to GIAS.
     */

    public static void ClearGroupSchools(this ISession session)
    {
        session.Remove(SessionConstants.GroupEstablishmentsKey);
    }

    public static IReadOnlyList<SelectedEstablishmentDataModel> GetGroupSchools(
        this ISession session
    )
    {
        return
            session.GetValue(SessionConstants.GroupEstablishmentsKey)
                is IEnumerable<SelectedEstablishmentDataModel> groupSchools
            ? groupSchools.ToArray()
            : [];
    }

    public static void SetGroupSchools(
        this ISession session,
        IEnumerable<SelectedEstablishmentDataModel> groupSchools
    )
    {
        session.SetValue(SessionConstants.GroupEstablishmentsKey, groupSchools);
    }

    /* **** SELECTED ESTABLISHMENTS ****
     * These are the dbo.establishment database IDs, URNs, and names of the school the user has selected.
     * School, SAT, and SSAT users should never have access to this.
     * For MAT users, it will represent the subset of schools within their trust which they have selected for a bulk operation.
     */

    public static void ClearSelectedEstablishments(this ISession session)
    {
        session.Remove(SessionConstants.SelectedEstablishmentsKey);
    }

    public static List<SelectedEstablishmentDataModel> GetSelectedSchools(this ISession session)
    {
        return
            session.GetValue(SessionConstants.SelectedEstablishmentsKey)
                is IEnumerable<SelectedEstablishmentDataModel> selectedEstablishments
            ? selectedEstablishments.ToList()
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
            EstablishmentId = id,
            Urn = urn,
            Name = name,
        };
        session.SetSelectedSchools([establishment]);
    }

    public static void SetSelectedSchools(
        this ISession session,
        IEnumerable<SelectedEstablishmentDataModel> selectedEstablishments
    )
    {
        foreach (var establishment in selectedEstablishments)
        {
            if (establishment.EstablishmentId < 1)
            {
                throw new InvalidDataException("Establishment ID cannot be less than 1");
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
}
