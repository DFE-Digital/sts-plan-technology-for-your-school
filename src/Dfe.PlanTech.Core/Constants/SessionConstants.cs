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
                { SelectedEstablishmentsKey, typeof(IEnumerable<SelectedEstablishmentDataModel>) },
                { FocusedEstablishmentKey, typeof(SelectedEstablishmentDataModel) },
                { GroupEstablishmentsKey, typeof(IEnumerable<SelectedEstablishmentDataModel>) },
                { UserEstablishmentKey, typeof(SqlEstablishmentDto) },
            }
        );
}
