using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class SessionConstants
{
    public const string FocusedEstablishmentKey = "focused_establishment";
    public const string GroupEstablishmentsKey = "group_establishments";
    public const string SelectedEstablishmentsKey = "selected_establishments";
    public const string UserEstablishmentKey = "user_establishment";

    public static readonly IReadOnlyDictionary<string, Type> SessionKeyTypes =
        new ReadOnlyDictionary<string, Type>(
            new Dictionary<string, Type>()
            {
                {
                    SelectedEstablishmentsKey,
                    typeof(IEnumerable<Models.SelectedEstablishmentDataModel>)
                },
                { FocusedEstablishmentKey, typeof(Models.SelectedEstablishmentDataModel) },
                {
                    GroupEstablishmentsKey,
                    typeof(IEnumerable<Models.SelectedEstablishmentDataModel>)
                },
                { UserEstablishmentKey, typeof(SqlEstablishmentDto) },
            }
        );
}
