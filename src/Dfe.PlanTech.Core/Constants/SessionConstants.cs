using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class SessionConstants
{
    public const string SelectedEstablishmentsKey = "selected_establishments";
    public const string FocusedEstablishmentKey = "focused_establishment";
    public static readonly IReadOnlyDictionary<string, Type> SessionKeyTypes =
        new ReadOnlyDictionary<string, Type>(
            new Dictionary<string, Type>()
            {
                { SelectedEstablishmentsKey, typeof(IEnumerable<int>) },
                { FocusedEstablishmentKey, typeof(int) },
            }
        );
}
